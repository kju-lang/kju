namespace KJU.Core.Intermediate.FunctionGeneration.PrologueEpilogue
{
    using System.Collections.Generic;
    using System.Linq;
    using AST.BuiltinTypes;
    using AST.Types;
    using CallGenerator;
    using NameMangler;
    using ReadWrite;

    public class PrologueEpilogueGenerator
    {
        private readonly ILabelFactory labelFactory;
        private readonly ReadWriteGenerator readWriteGenerator;
        private readonly CallGenerator callGenerator;
        private readonly Dictionary<HardwareRegister, VirtualRegister> calleeSavedMapping;

        public PrologueEpilogueGenerator(
            ILabelFactory labelFactory,
            CallGenerator callGenerator,
            ReadWriteGenerator readWriteGenerator)
        {
            this.labelFactory = labelFactory;
            this.readWriteGenerator = readWriteGenerator;
            this.callGenerator = callGenerator;
            // RBP is handled separately, since it has a set place on the stack frame
            this.calleeSavedMapping = HardwareRegisterUtils.CalleeSavedRegisters
                .Where(reg => reg != HardwareRegister.RBP)
                .ToDictionary(register => register, register => new VirtualRegister());
        }

        public ILabel GeneratePrologue(Function.Function function, ILabel after)
        {
            var prologue = new List<Node>
                {
                    new UsesDefinesNode(null, HardwareRegisterUtils.CalleeSavedRegisters),

                    new Comment("Save RBP - parent base pointer"),
                    new Push(new RegisterRead(HardwareRegister.RBP)),

                    new Comment("Copy RSP to RBP - current base pointer"),
                    this.readWriteGenerator.GenerateWrite(
                        function,
                        HardwareRegister.RBP,
                        new RegisterRead(HardwareRegister.RSP)),

                    new Comment("Place pointer to function's stack layout (list of variables which are pointers)"),
                    new PushStackLayoutPointer(function),

                    new Comment("Stack 16 alignment"),
                    new AlignStackPointer(8),

                    new Comment("Reserve memory for local variables"),
                    new ReserveStackMemory(function),
                };

            prologue.Add(new Comment("Save callee saved registers."));
            prologue.AddRange(
                    this.calleeSavedMapping.Select(
                        kvp =>
                            this.readWriteGenerator.GenerateWrite(
                                function,
                                kvp.Value,
                                new RegisterRead(kvp.Key))));

            prologue.Add(new Comment("Retrieve arguments to temporary variables."));

            List<Node> sources = this.GetArgumentSources(function);
            List<ILocation> targets = this.GetArgumentTargets(function);
            var temporaries = targets.Select(loc =>
            {
                if (loc is HeapLocation heapLocation)
                    return function.ReserveStackFrameLocation(heapLocation.Type);
                else
                    return loc;
            }).ToList();
            prologue.AddRange(sources
                              .Zip(temporaries, (value, variable) => this.readWriteGenerator.GenerateWrite(function, variable, value)));

            List<Node> copyTemporariesToClosure = new List<Node>() { new Comment("Copy temporaries to closure") };
            copyTemporariesToClosure.AddRange(
                temporaries.Zip(targets, (temporary, target) =>
                {
                    if (temporary == target)
                        return new Comment("copy not needed");
                    else
                        return this.readWriteGenerator.GenerateWrite(function, target, this.readWriteGenerator.GenerateRead(function, temporary));
                }));
            copyTemporariesToClosure.Add(new Comment("Function body"));
            ILabel afterClosureAlloc = copyTemporariesToClosure.MakeTreeChain(this.labelFactory, after);

            prologue.Add(new Comment("Allocate closure: " + function.ClosureType));
            ILabel closureGen = this.AllocateStruct(function, function.ClosureType, function.ClosurePointer, afterClosureAlloc);

            return prologue.MakeTreeChain(this.labelFactory, closureGen);
        }

        public ILabel AllocateStruct(Function.Function function, StructType structType, ILocation target, ILabel after)
        {
            if (structType.Fields.Count == 0)
                return after;

            var allocateFunctionName = NameMangler.GetMangledName("allocate", new List<AST.DataType>() { IntType.Instance }, null);

            var allocateFunc = new Function.Function(
                null,
                allocateFunctionName,
                new List<AST.VariableDeclaration>() { new AST.VariableDeclaration(null, IntType.Instance, "size", null) },
                isEntryPoint: false,
                isForeign: true);

            var sizeRegister = new VirtualRegister();

            var call = this.callGenerator.GenerateCall(target, new List<VirtualRegister>() { sizeRegister }, after, callerFunction: function, function: allocateFunc);

            var writeSize = new RegisterWrite(sizeRegister, new IntegerImmediateValue(structType.Fields.Count * 8));
            var startLabel = this.labelFactory.GetLabel(new Tree(writeSize, new UnconditionalJump(call)));

            return startLabel;
        }

        public ILabel GenerateEpilogue(Function.Function function, Node retVal)
        {
            var operations =
                new List<Node> { new Comment("Restore callee saved registers.") }
                    .Concat(
                        this.calleeSavedMapping
                            .Select(
                                kvp =>
                                    this.readWriteGenerator.GenerateWrite(
                                        function,
                                        kvp.Key,
                                        new RegisterRead(kvp.Value))))
                    .Append(new Comment("Save result to RAX"))
                    .Append(new RegisterWrite(HardwareRegister.RAX, retVal))
                    .Append(new Comment("Restore RSP from RBP"))
                    .Append(
                        this.readWriteGenerator.GenerateWrite(
                            function,
                            HardwareRegister.RSP,
                            new RegisterRead(HardwareRegister.RBP)))
                    .Append(new Comment("Restore RBP from stack"))
                    .Append(new Pop(HardwareRegister.RBP))
                    .Append(new Comment("Clear direction flag."))
                    .Append(new ClearDF())
                    .Append(
                        new UsesDefinesNode(
                            HardwareRegisterUtils.CalleeSavedRegisters,
                            new List<VirtualRegister> { HardwareRegister.RSP }));

            return operations.MakeTreeChain(this.labelFactory, new Ret());
        }

        private List<ILocation> GetArgumentTargets(Function.Function function)
        {
            List<ILocation> arguments = function.Parameters
                .Select(parameter => parameter.IntermediateVariable).ToList();

            if (function.Parent != null)
                arguments.Add(function.Link);

            return arguments;
        }

        private List<Node> GetArgumentSources(Function.Function function)
        {
            var registerArguments = HardwareRegisterUtils
                .ArgumentRegisters
                .Select(reg => new RegisterRead(reg));

            var memoryArguments = Enumerable.Range(0, function.GetStackArgumentsCount())
                .Select(n => (Node)new MemoryRead(HardwareRegister.RBP.OffsetAddress(n + 2)));

            return registerArguments.Concat(memoryArguments).ToList();
        }
    }
}

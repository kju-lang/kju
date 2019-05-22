namespace KJU.Core.Intermediate.FunctionGeneration.PrologueEpilogue
{
    using System.Collections.Generic;
    using System.Linq;
    using ReadWrite;

    public class PrologueEpilogueGenerator
    {
        private readonly ILabelFactory labelFactory;
        private readonly ReadWriteGenerator readWriteGenerator;
        private readonly Dictionary<HardwareRegister, VirtualRegister> calleeSavedMapping;

        public PrologueEpilogueGenerator(
            ILabelFactory labelFactory,
            ReadWriteGenerator readWriteGenerator)
        {
            this.labelFactory = labelFactory;
            this.readWriteGenerator = readWriteGenerator;
            // RBP is handled separately, since it has a set place on the stack frame
            this.calleeSavedMapping = HardwareRegisterUtils.CalleeSavedRegisters
                .Where(reg => reg != HardwareRegister.RBP)
                .ToDictionary(register => register, register => new VirtualRegister());
        }

        public ILabel GeneratePrologue(Function.Function function, ILabel after)
        {
            return new List<Node>
                {
                    new UsesDefinesNode(null, HardwareRegisterUtils.CalleeSavedRegisters),
                    new Comment("Save RBP - parent base pointer"),
                    new Push(new RegisterRead(HardwareRegister.RBP)),
                    new Comment("Copy RSP to RBP - current base pointer"),
                    this.readWriteGenerator.GenerateWrite(
                        function,
                        HardwareRegister.RBP,
                        new RegisterRead(HardwareRegister.RSP)),
                    new Comment("Reserve memory for local variables"),
                    new ReserveStackMemory(function),
                }.Append(new Comment("Save callee saved registers."))
                .Concat(
                    this.calleeSavedMapping.Select(
                        kvp =>
                            this.readWriteGenerator.GenerateWrite(
                                function,
                                kvp.Value,
                                new RegisterRead(kvp.Key))))
                .Append(new Comment("Retrieve arguments."))
                .Concat(this.RetrieveArguments(function))
                .Append(new Comment("Function body."))
                .MakeTreeChain(this.labelFactory, after);
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

        private IEnumerable<Node> RetrieveArguments(Function.Function function)
        {
            var registerArguments = HardwareRegisterUtils
                .ArgumentRegisters
                .Select(reg => new RegisterRead(reg));

            var memoryArguments = Enumerable.Range(0, function.GetStackArgumentsCount())
                .Select(n => (Node)new MemoryRead(HardwareRegister.RBP.OffsetAddress(n + 2)));

            var argumentsVirtualRegisters = registerArguments
                .Concat(memoryArguments);

            return function.Parameters
                .Select(parameter => parameter.IntermediateVariable)
                .Append(function.Link)
                .Zip(
                    argumentsVirtualRegisters,
                    (variable, value) => this.readWriteGenerator.GenerateWrite(function, variable, value))
                .Reverse();
        }
    }
}
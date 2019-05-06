#pragma warning disable CS0169
#pragma warning disable SA1202 // this class violates SRP, so it's exempt from this warning :) (private members must come before public)
namespace KJU.Core.Intermediate.Function
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using AST.VariableAccessGraph;
    using CodeGeneration.FunctionToAsmGeneration;

    public class Function : IFunction
    {
        private readonly ILabelFactory labelFactory = new LabelFactory(new LabelIdGuidGenerator());
        private readonly Dictionary<HardwareRegister, VirtualRegister> calleeSavedMapping;
        private readonly Variable link;
        private readonly Function parent;
        private readonly List<Variable> arguments;

        public Function(
            Function parent,
            string mangledName,
            IEnumerable<AST.VariableDeclaration> parameters)
        {
            // RBP is handled separately, since it has a set place on the stack frame
            this.calleeSavedMapping = HardwareRegisterUtils.CalleeSavedRegisters
                .Where(reg => reg != HardwareRegister.RBP)
                .ToDictionary(register => register, register => new VirtualRegister());
            this.link = new Variable(this, this.ReserveStackFrameLocation());
            this.parent = parent;
            this.MangledName = mangledName;
            this.arguments = parameters.Select(parameter =>
            {
                var location = new VirtualRegister(); // Why not virtual register?
                var variable = new Variable(this, location);
                parameter.IntermediateVariable = variable;
                return variable;
            }).ToList();
        }

        public string MangledName { get; }

        public int StackBytes { get; set; }

        private int StackArgumentsCount =>
            Math.Max(0, this.arguments.Count + 1 - HardwareRegisterUtils.ArgumentRegisters.Count);

        public MemoryLocation ReserveStackFrameLocation()
        {
            return new MemoryLocation(this, -(this.StackBytes += 8));
        }

        // We will use standard x86-64 conventions -> RDI, RSI, RDX, RCX, R8, R9.
        // TODO: instruction templates covering hw register modifications
        public ILabel GenerateCall(
            VirtualRegister result,
            IEnumerable<VirtualRegister> callArguments,
            ILabel onReturn,
            Function caller)
        {
            var savedRsp = new VirtualRegister();

            var preCall = this.RspAlignmentNodes(savedRsp)
                .Concat(this.PassArguments(caller, callArguments))
                .Append(new ClearDF())
                .Append(new UsesDefinesNode(null, HardwareRegisterUtils.CallerSavedRegisters));

            var postCall = new List<Node>
            {
                result.CopyFrom(HardwareRegister.RAX),
                HardwareRegister.RSP.CopyFrom(savedRsp),
            };

            var controlFlow = new FunctionCall(this, postCall.MakeTreeChain(this.labelFactory, onReturn));
            return preCall.MakeTreeChain(this.labelFactory, controlFlow);
        }

        public ILabel GeneratePrologue(ILabel after)
        {
            var operations = new List<Node>()
                {
                    new UsesDefinesNode(null, HardwareRegisterUtils.CalleeSavedRegisters),
                    new Comment("Save RBP - parent base pointer"),
                    new Push(new RegisterRead(HardwareRegister.RBP)),
                    new Comment("Copy RSP to RBP - current base pointer"),
                    HardwareRegister.RBP.CopyFrom(HardwareRegister.RSP),
                    new Comment("Reserve memory for local variables"),
                    new ReserveStackMemory(this),
                }.Append(new Comment("Save callee saved registers."))
                .Concat(this.calleeSavedMapping.Select(kvp => kvp.Value.CopyFrom(kvp.Key)))
                .Append(new Comment("Retrieve arguments."))
                .Concat(this.RetrieveArguments());

            return operations.MakeTreeChain(this.labelFactory, after);
        }

        public ILabel GenerateEpilogue(Node retVal)
        {
            var operations =
                new List<Node> { new Comment("Restore callee saved registers.") }
                    .Concat(this.calleeSavedMapping
                        .Select(kvp => kvp.Key.CopyFrom(kvp.Value)))
                    .Append(new Comment("Save result to RAX"))
                    .Append(new RegisterWrite(HardwareRegister.RAX, retVal))
                    .Append(new Comment("Restore RSP from RBP"))
                    .Append(HardwareRegister.RSP.CopyFrom(HardwareRegister.RBP))
                    .Append(new Comment("Restore RBP from stack"))
                    .Append(new Pop(HardwareRegister.RBP))
                    .Append(new Comment("Clear direction flag."))
                    .Append(new ClearDF())
                    .Append(new UsesDefinesNode(HardwareRegisterUtils.CalleeSavedRegisters, new List<VirtualRegister> { HardwareRegister.RSP }));

            return operations.MakeTreeChain(this.labelFactory, new Ret());
        }

        private Function GetCallingSibling(Function caller)
        {
            var result = caller;
            while (result.parent != this.parent)
            {
                result = result.parent;
            }

            return result;
        }

        private IEnumerable<Node> RspAlignmentNodes(VirtualRegister savedRsp)
        {
            return new List<Node>
            {
                savedRsp.CopyFrom(HardwareRegister.RSP),
                new AlignStackPointer(offsetByQword: this.StackArgumentsCount % 2 == 1),
            };
        }

/*
        Argument position on wrt. stack frame (if needed):
               |             ...            |
               | (i+7)th argument           | rbp + 16 + 8i
               |             ...            |
               | 7th argument               | rbp + 16
               | return stack pointer value | rbp + 8
        rbp -> | previous rbp value         |
        Static link is the last argument, either in register or on stack.
*/

        private IEnumerable<Node> PassArguments(Function caller, IEnumerable<VirtualRegister> argRegisters)
        {
            var readStaticLink = caller == this.parent
                ? new RegisterRead(HardwareRegister.RBP)
                : caller.GenerateRead(this.GetCallingSibling(caller).link);

            var values = argRegisters
                .Select(argVR => new RegisterRead(argVR))
                .Append(readStaticLink).ToList();

            return values.Zip(
                    HardwareRegisterUtils.ArgumentRegisters,
                    (value, hwReg) => new RegisterWrite(hwReg, value))
                .Concat<Node>(
                    values.Skip(HardwareRegisterUtils.ArgumentRegisters.Count).Reverse()
                        .Select(value => new Push(value)));
        }

        private IEnumerable<Node> RetrieveArguments()
        {
            var registerArguments = HardwareRegisterUtils
                .ArgumentRegisters
                .Select(reg => new RegisterRead(reg));

            var memoryArguments = Enumerable.Range(0, this.StackArgumentsCount)
                .Select(n => (Node)new MemoryRead(HardwareRegister.RBP.OffsetAddress(n + 2)));

            var argumentsVirtualRegisters = registerArguments
                .Concat(memoryArguments);

            return this.arguments
                .Append(this.link)
                .Zip(argumentsVirtualRegisters, this.GenerateWrite);
        }

        public Node GenerateRead(Variable variable)
        {
            return this.GenerateRead(variable, new RegisterRead(HardwareRegister.RBP));
        }

        public Node GenerateWrite(Variable variable, Node value)
        {
            return this.GenerateWrite(variable, value, new RegisterRead(HardwareRegister.RBP));
        }

        private Node GenerateRead(Variable variable, Node framePointer)
        {
            switch (variable.Location)
            {
                case VirtualRegister virtualRegister:
                    if (variable.Owner != this)
                    {
                        throw new ArgumentException("Read of virtual register outside its function");
                    }

                    return new RegisterRead(virtualRegister);
                case MemoryLocation location:
                    return new MemoryRead(this.GenerateVariableLocation(location, framePointer));
                default:
                    throw new ArgumentException($"Unexpected Location kind {variable}");
            }
        }

        private Node GenerateWrite(Variable variable, Node value, Node framePointer)
        {
            switch (variable.Location)
            {
                case VirtualRegister virtualRegister:
                    if (variable.Owner != this)
                    {
                        throw new Exception("Write to virtual register outside its function");
                    }

                    return new RegisterWrite(virtualRegister, value);
                case MemoryLocation location:
                    return new MemoryWrite(this.GenerateVariableLocation(location, framePointer), value);
                default:
                    throw new ArgumentException($"Unexpected Location kind {variable}");
            }
        }

        private Node GenerateVariableLocation(MemoryLocation loc, Node framePointer)
        {
            if (loc.Function == this)
            {
                return new ArithmeticBinaryOperation(
                    AST.ArithmeticOperationType.Addition,
                    framePointer,
                    new IntegerImmediateValue(loc.Offset));
            }

            if (this.parent == null)
            {
                throw new ArgumentException("Variable not found in parents chain");
            }

            var parentFramePointer = this.GenerateRead(this.link, framePointer);
            return this.parent.GenerateVariableLocation(loc, parentFramePointer);
        }

        public ILabel GenerateBody(AST.FunctionDeclaration root)
        {
            var variableAccessGraphGenerator =
                new VariableAccessGraphGeneratorFactory().GetGenerator(); // TODO dependency injection
            this.ExtractTemporaryVariables(root, variableAccessGraphGenerator);
            var generator = new FunctionBodyGenerator.FunctionBodyGenerator(this, this.labelFactory);
            return generator.BuildFunctionBody(root.Body);
        }

        private void ExtractTemporaryVariables(
            AST.FunctionDeclaration root, IVariableAccessGraphGenerator variableAccessGraphGenerator)
        {
            var variableAccess =
                variableAccessGraphGenerator.GetVariableInfoPerAstNode(root);
            var extractor = new TemporaryVariablesExtractor.TemporaryVariablesExtractor(variableAccess, this);
            var result = extractor.ExtractTemporaryVariables(root.Body);
            var instructions = result.Concat(root.Body.Instructions).ToList();
            root.Body = new AST.InstructionBlock(instructions);
        }
    }
}
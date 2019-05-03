#pragma warning disable CS0169
#pragma warning disable SA1202 // this class violates SRP, so it's exempt from this warning :) (private members must come before public)
namespace KJU.Core.Intermediate
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using AST.VariableAccessGraph;
    using static CFGUtils;
    using static HardwareRegisterUtils;

    public class Function
    {
        private Dictionary<HardwareRegister, VirtualRegister> calleeSavedMapping;

        public Function(Function parent)
            : this()
        {
            this.Parent = parent;
        }

        public Function()
        {
            // RBP is handled separately, since it has a set place on the stack frame
            this.calleeSavedMapping = HardwareRegisterUtils.CalleeSavedRegisters
                .Where(reg => reg != HardwareRegister.RBP)
                .ToDictionary(register => register, register => new VirtualRegister());
        }

        public Variable Link { get; set; }

        public List<Variable> Arguments { get; set; }

        public Function Parent { get; set; }

        public string MangledName { get; set; }

        public int StackBytes { get; set; }

        private int StackArgumentsCount
        {
            get { return Math.Max(0, this.Arguments.Count + 1 - ArgumentRegisters.Count); }
        }

        public MemoryLocation ReserveStackFrameLocation()
        {
            return new MemoryLocation(this, -(this.StackBytes += 8));
        }

        // We will use standard x86-64 conventions -> RDI, RSI, RDX, RCX, R8, R9.
        // TODO: instruction templates covering hw register modifications
        public Label GenerateCall(
            VirtualRegister result,
            List<VirtualRegister> arguments,
            Label onReturn,
            Function caller)
        {
            var savedRsp = new VirtualRegister();

            IEnumerable<Node> preCall = this.RspAlignmentNodes(savedRsp)
                .Concat(this.PassArguments(caller, arguments))
                .Append(new ClearDF());

            IEnumerable<Node> postCall = new List<Node>
            {
                RegisterCopy(result, HardwareRegister.RAX),
                RegisterCopy(HardwareRegister.RSP, savedRsp),
            };

            Tree call = new Tree(
                new UnitImmediateValue(),
                new FunctionCall(this, MakeTreeChain(postCall, onReturn)));

            return MakeTreeChain(preCall, call);
        }

        public Label GeneratePrologue(Label after)
        {
            var operations = new List<Node>()
            {
                new Push(new RegisterRead(HardwareRegister.RBP)),
                RegisterCopy(HardwareRegister.RBP, HardwareRegister.RSP),
                new ReserveStackMemory(this),
            }
                .Concat(this.calleeSavedMapping.Select(kvp => RegisterCopy(kvp.Value, kvp.Key)))
                .Concat(this.RetrieveArguments());

            return MakeTreeChain(operations, after);
        }

        public Label GenerateEpilogue(Node retVal)
        {
            var operations = this.calleeSavedMapping
                .Select(kvp => RegisterCopy(kvp.Key, kvp.Value))
                .Append(new RegisterWrite(HardwareRegister.RAX, retVal))
                .Append(RegisterCopy(HardwareRegister.RSP, HardwareRegister.RBP))
                .Append(new Pop(HardwareRegister.RBP))
                .Append(new ClearDF());

            Tree ret = new Tree(new UnitImmediateValue(), new Ret());
            return MakeTreeChain(operations, ret);
        }

        private Function GetCallingSibling(Function caller)
        {
            var result = caller;
            while (result.Parent != this.Parent)
            {
                result = result.Parent;
            }

            return result;
        }

        private IEnumerable<Node> RspAlignmentNodes(VirtualRegister savedRsp)
        {
            return new List<Node>
            {
                RegisterCopy(savedRsp, HardwareRegister.RSP),
                new AlignStackPointer(offsetByQword: this.StackArgumentsCount % 2 == 1),
            };
        }

        // Argument position on wrt. stack frame (if needed):
        //
        //        |             ...            |
        //        | (i+7)th argument           | rbp + 16 + 8i
        //        |             ...            |
        //        | 7th argument               | rbp + 16
        //        | return stack pointer value | rbp + 8
        // rbp -> | previous rbp value         |
        //
        // Static link is the last argument, either in register or on stack.

        private IEnumerable<Node> PassArguments(Function caller, IEnumerable<VirtualRegister> argRegisters)
        {
            Node readStaticLink = caller == this.Parent
                ? new RegisterRead(HardwareRegister.RBP)
                : caller.GenerateRead(this.GetCallingSibling(caller).Link);

            var values = argRegisters
                .Select(argVR => new RegisterRead(argVR))
                .Append(readStaticLink);

            return Enumerable.Concat<Node>(
                values.Zip(ArgumentRegisters, (value, hwReg) => new RegisterWrite(hwReg, value)),
                values.Skip(ArgumentRegisters.Count).Reverse().Select(value => new Push(value)));
        }

        private IEnumerable<Node> RetrieveArguments()
        {
            Func<int, Node> readNthStackArg = n => new MemoryRead(OffsetAddress(HardwareRegister.RBP, n + 2));

            var values = Enumerable.Concat<Node>(
                ArgumentRegisters.Select(reg => new RegisterRead(reg)),
                Enumerable.Range(0, this.StackArgumentsCount).Select(readNthStackArg));

            return this.Arguments
                .Append(this.Link)
                .Zip(values, this.GenerateWrite);
        }

        public Node GenerateRead(Variable v)
        {
            return this.GenerateRead(v, new RegisterRead(HardwareRegister.RBP));
        }

        public Node GenerateWrite(Variable v, Node value)
        {
            return this.GenerateWrite(v, value, new RegisterRead(HardwareRegister.RBP));
        }

        private Node GenerateRead(Variable v, Node framePointer)
        {
            switch (v.Location)
            {
                case VirtualRegister reg:
                    if (v.Owner != this)
                        throw new ArgumentException("read of virtual register outside its function");
                    return new RegisterRead(reg);
                case MemoryLocation location:
                    return new MemoryRead(this.GenerateVariableLocation(location, framePointer));
                default:
                    throw new ArgumentException($"unexpected Location kind {v}");
            }
        }

        private Node GenerateWrite(Variable v, Node value, Node framePointer)
        {
            switch (v.Location)
            {
                case VirtualRegister reg:
                    Debug.Assert(v.Owner == this, "write to virtual register outside its function");
                    return new RegisterWrite(reg, value);
                case MemoryLocation location:
                    return new MemoryWrite(this.GenerateVariableLocation(location, framePointer), value);
                default:
                    throw new ArgumentException($"unexpected Location kind {v}");
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
            else
            {
                if (this.Parent == null)
                    throw new ArgumentException("variable not found in parents chain");

                Node parentFramePointer = this.GenerateRead(this.Link, framePointer);
                return this.Parent.GenerateVariableLocation(loc, parentFramePointer);
            }
        }

        public Label GenerateBody(AST.FunctionDeclaration root)
        {
            this.ExtractTemporaryVariables(root);
            var generator = new FunctionBodyGenerator.FunctionBodyGenerator(this);
            return generator.BuildFunctionBody(root.Body);
        }

        private void ExtractTemporaryVariables(AST.FunctionDeclaration root)
        {
            var variableAccessGraphGenerator = new VariableAccessGraphGenerator(new AST.CallGraph.CallGraphGenerator());
            var variableModificationGraph = variableAccessGraphGenerator.BuildVariableModificationsPerAstNode(root);
            var variableAccessGraph = variableAccessGraphGenerator.BuildVariableAccessesPerAstNode(root);
            var extractor = new TemporaryVariablesExtractor.TemporaryVariablesExtractor(variableModificationGraph, variableAccessGraph);
            var result = extractor.ExtractTemporaryVariables(root.Body);
            var instructions = result.Concat(root.Body.Instructions).ToList();
            root.Body = new AST.InstructionBlock(instructions);
        }
    }
}
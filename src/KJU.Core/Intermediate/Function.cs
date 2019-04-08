#pragma warning disable CS0169
#pragma warning disable SA1202 // this class violates SRP, so it's exempt from this warning :) (private members must come before public)
namespace KJU.Core.Intermediate
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using AST.VariableAccessGraph;

    public class Function
    {
        private int stackBytes;

        public Variable Link { get; set; }

        public List<Variable> Arguments { get; set; }

        public Function Parent { get; set; }

        public MemoryLocation ReserveStackFrameLocation()
        {
            return new MemoryLocation(this, -(this.stackBytes += 8));
        }

        public Label GenerateCall(VirtualRegister result, List<VirtualRegister> args, Label onReturn, Function caller)
        {
            throw new NotImplementedException();
        }

        public Node GenerateRead(Variable v)
        {
            return this.GenerateRead(v, new RegisterRead(new HardwareRegister(HardwareRegisterName.RBP)));
        }

        public Node GenerateWrite(Variable v, Node value)
        {
            return this.GenerateWrite(v, value, new RegisterRead(new HardwareRegister(HardwareRegisterName.RBP)));
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

        public Label GeneratePrologue(Label after)
        {
            throw new NotImplementedException();
        }

        public Label GenerateEpilogue(Tree retVal, Label after)
        {
            throw new NotImplementedException();
        }

        public Label GenerateBody(Label after, AST.FunctionDeclaration root)
        {
            throw new NotImplementedException();
        }

        private void ExtractTemporaryVariables(AST.FunctionDeclaration root)
        {
            var variableAccessGraphGenerator = new VariableAccessGraphGenerator(new AST.CallGraph.CallGraphGenerator());
            var variableModificationGraph = variableAccessGraphGenerator.BuildVariableModificationsPerAstNode(root);
            var variableAccessGraph = variableAccessGraphGenerator.BuildVariableAccessesPerAstNode(root);
            var extractor = new TemporaryVariablesExtractor(variableModificationGraph, variableAccessGraph);
            var result = extractor.ExtractTemporaryVariables(root.Body);
            var instructions = result.Concat(root.Body.Instructions).ToList();
            root.Body = new AST.InstructionBlock(instructions);
        }
    }
}
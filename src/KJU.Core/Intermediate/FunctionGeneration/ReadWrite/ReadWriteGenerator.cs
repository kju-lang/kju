namespace KJU.Core.Intermediate.FunctionGeneration.ReadWrite
{
    using System;

    public class ReadWriteGenerator
    {
        public Node GenerateRead(Function.Function function, ILocation variable)
        {
            return this.GenerateRead(function, variable, new RegisterRead(HardwareRegister.RBP));
        }

        public Node GenerateWrite(Function.Function function, ILocation variable, Node value)
        {
            return this.GenerateWrite(function, variable, value, new RegisterRead(HardwareRegister.RBP));
        }

        public Node GenerateRead(Function.Function function, ILocation variable, Node framePointer)
        {
            switch (variable)
            {
                case VirtualRegister virtualRegister:
                    return new RegisterRead(virtualRegister);
                case MemoryLocation memoryLocation:
                    var address = this.GenerateVariableLocation(function, memoryLocation, framePointer);
                    return new MemoryRead(address);
                default:
                    throw new ArgumentException($"Unexpected Location kind {variable}");
            }
        }

        public Node GenerateWrite(Function.Function function, ILocation variable, Node value, Node framePointer)
        {
            switch (variable)
            {
                case VirtualRegister virtualRegister:
                    return new RegisterWrite(virtualRegister, value);
                case MemoryLocation location:
                    return new MemoryWrite(this.GenerateVariableLocation(function, location, framePointer), value);
                default:
                    throw new ArgumentException($"Unexpected Location kind {variable}");
            }
        }

        private Node GenerateVariableLocation(Function.Function function, MemoryLocation loc, Node framePointer)
        {
            if (loc.Function == function)
            {
                return new ArithmeticBinaryOperation(
                    AST.ArithmeticOperationType.Addition,
                    framePointer,
                    new IntegerImmediateValue(loc.Offset));
            }

            if (function.Parent == null)
            {
                throw new ArgumentException("Variable not found in parents chain");
            }

            var parentFramePointer = this.GenerateRead(function, function.Link, framePointer);
            return this.GenerateVariableLocation(function.Parent, loc, parentFramePointer);
        }
    }
}
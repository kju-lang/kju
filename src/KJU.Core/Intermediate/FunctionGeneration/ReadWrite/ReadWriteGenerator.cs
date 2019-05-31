namespace KJU.Core.Intermediate.FunctionGeneration.ReadWrite
{
    using System;

    public class ReadWriteGenerator
    {
        public Node GenerateRead(Function.Function function, ILocation variable)
        {
            return this.GenerateRead(function, variable, this.GenerateClosurePointer(function));
        }

        public Node GenerateWrite(Function.Function function, ILocation variable, Node value)
        {
            return this.GenerateWrite(function, variable, value, this.GenerateClosurePointer(function));
        }

        private Node GenerateClosurePointer(Function.Function function)
        {
            return new MemoryRead(this.GenerateStackVariableLocation(function, function.ClosurePointer));
        }

        private Node GenerateRead(Function.Function function, ILocation variable, Node closurePointer)
        {
            switch (variable)
            {
                case VirtualRegister virtualRegister:
                    return new RegisterRead(virtualRegister);
                case MemoryLocation memoryLocation:
                    var stackAddress = this.GenerateStackVariableLocation(function, memoryLocation);
                    return new MemoryRead(stackAddress);
                case HeapLocation heapLocation:
                    var heapAddress = this.GenerateHeapVariableLocation(function, heapLocation, closurePointer);
                    return new MemoryRead(heapAddress);
                default:
                    throw new ArgumentException($"Unexpected Location kind {variable}");
            }
        }

        private Node GenerateWrite(Function.Function function, ILocation variable, Node value, Node closurePointer)
        {
            switch (variable)
            {
                case VirtualRegister virtualRegister:
                    return new RegisterWrite(virtualRegister, value);
                case MemoryLocation location:
                    return new MemoryWrite(this.GenerateStackVariableLocation(function, location), value);
                case HeapLocation heapLocation:
                    var address = this.GenerateHeapVariableLocation(function, heapLocation, closurePointer);
                    return new MemoryWrite(address, value);
                default:
                    throw new ArgumentException($"Unexpected Location kind {variable}");
            }
        }

        private Node GenerateHeapVariableLocation(Function.Function function, HeapLocation loc, Node closurePointer)
        {
            if (loc.Function == function)
            {
                return new ArithmeticBinaryOperation(
                    AST.ArithmeticOperationType.Addition,
                    closurePointer,
                    new IntegerImmediateValue(loc.Offset));
            }

            if (function.Parent == null)
            {
                throw new ArgumentException("Variable not found in parents chain");
            }

            var parentClosurePointer = this.GenerateRead(function, function.Link, closurePointer);
            return this.GenerateHeapVariableLocation(function.Parent, loc, parentClosurePointer);
        }

        private Node GenerateStackVariableLocation(Function.Function function, MemoryLocation loc)
        {
            if (loc.Function == function)
            {
                return new ArithmeticBinaryOperation(
                    AST.ArithmeticOperationType.Addition,
                    new RegisterRead(HardwareRegister.RBP),
                    new IntegerImmediateValue(loc.Offset));
            }

            throw new ArgumentException($"Attempt to access non-local stack variable of {function}");
        }
    }
}

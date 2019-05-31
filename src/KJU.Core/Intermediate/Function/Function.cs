namespace KJU.Core.Intermediate.Function
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using KJU.Core.AST;
    using KJU.Core.AST.BuiltinTypes;
    using KJU.Core.AST.Types;

    public class Function
    {
        private List<(int offset, DataType target)> stackLayoutInfo = new List<(int offset, DataType target)>();

        private List<StructField> closureStructFields = new List<StructField>();

        public Function(
            Function parent,
            string mangledName,
            IReadOnlyList<AST.VariableDeclaration> parameters,
            bool isEntryPoint,
            bool isForeign,
            bool hasChildFunctions = false)
        {
            this.Parent = parent;
            this.MangledName = mangledName;
            this.IsForeign = isForeign;
            this.IsEntryPoint = isEntryPoint;
            this.Parameters = parameters;

            // We are pushing a stack layout label on the stack.
            this.StackBytes = 8;
            this.ClosureType = new StructType("closure", this.closureStructFields);
            this.ClosurePointer = this.ReserveStackFrameLocation(this.ClosureType); // this needs to be on stack, so GC works

            if (this.Parent != null)
            {
                if (hasChildFunctions)
                    this.Link = this.ReserveClosureLocation(".parent", this.Parent.ClosureType);
                else
                    this.Link = this.ReserveStackFrameLocation(this.Parent.ClosureType);
            }
        }

        public IReadOnlyList<VariableDeclaration> Parameters { get; }

        public Function Parent { get; }

        public string MangledName { get; }

        public string LayoutLabel { get => $"{this.MangledName}_stack_layout"; }

        public int StackBytes { get; private set; }

        public bool IsForeign { get; }

        public bool IsEntryPoint { get; }

        public StructType ClosureType { get; }

        public MemoryLocation ClosurePointer { get; }

        public ILocation Link { get; }

        [SuppressMessage(
            "StyleCop.CSharp.ReadabilityRules",
            "SA1101:PrefixLocalCallsWithThis",
            Justification = "Shows false warning when named tuples are used.")]
            public MemoryLocation ReserveStackFrameLocation(DataType dataType)
        {
            this.StackBytes += 8;
            if (dataType is ArrayType || dataType is StructType)
            {
                this.stackLayoutInfo.Add((offset: -this.StackBytes / 8, target: dataType));
            }

            return new MemoryLocation(this, -this.StackBytes);
        }

        public HeapLocation ReserveClosureLocation(string name, DataType dataType)
        {
            this.closureStructFields.Add(new StructField(inputRange: null, name: name, dataType));
            return new HeapLocation(this, (this.closureStructFields.Count - 1) * 8, dataType);
        }

        public IEnumerable<string> GenerateStackLayout()
        {
            return this.stackLayoutInfo
                .Select(pointer => $"dq {pointer.offset}, {pointer.target.LayoutLabel}")
                .Append("dq 0, 0");
        }
    }
}

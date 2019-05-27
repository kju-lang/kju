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

        public Function(
            Function parent,
            string mangledName,
            IReadOnlyList<AST.VariableDeclaration> parameters,
            bool isEntryPoint,
            bool isForeign)
        {
            this.Parent = parent;
            this.MangledName = mangledName;
            this.IsForeign = isForeign;
            this.IsEntryPoint = isEntryPoint;
            this.Parameters = parameters;

            // Only a quick fix. It's because we are pushing an stack layout label on the stack.
            this.StackBytes = 8;
            this.Link = this.ReserveStackFrameLocation(IntType.Instance);
        }

        public IReadOnlyList<VariableDeclaration> Parameters { get; }

        public ILocation Link { get; }

        public Function Parent { get; }

        public string MangledName { get; }

        public string LayoutLabel { get => $"{this.MangledName}_stack_layout"; }

        public int StackBytes { get; private set; }

        public bool IsForeign { get; }

        public bool IsEntryPoint { get; }

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

        public IEnumerable<string> GenerateStackLayout()
        {
            return this.stackLayoutInfo
                .Select(pointer => $"dq {pointer.offset}, {pointer.target.LayoutLabel}")
                .Append("dq 0, 0");
        }
    }
}

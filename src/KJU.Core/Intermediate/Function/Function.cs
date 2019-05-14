namespace KJU.Core.Intermediate.Function
{
    using System.Collections.Generic;

    public class Function
    {
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
            this.StackBytes = 0;
            this.Link = this.ReserveStackFrameLocation();
        }

        public ILocation Link { get; }

        public Function Parent { get; }

        public string MangledName { get; }

        public int StackBytes { get; private set; }

        public bool IsForeign { get; }

        public bool IsEntryPoint { get; }

        public IReadOnlyList<AST.VariableDeclaration> Parameters { get; }

        public MemoryLocation ReserveStackFrameLocation()
        {
            return new MemoryLocation(this, -(this.StackBytes += 8));
        }
    }
}
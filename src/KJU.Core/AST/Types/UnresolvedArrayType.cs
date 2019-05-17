namespace KJU.Core.AST.Types
{
    public class UnresolvedArrayType : DataType
    {
        public UnresolvedArrayType(DataType child)
        {
            this.Child = child;
        }

        public DataType Child { get; }

        public override string ToString()
        {
            return $"UnresolvedArray [{this.Child.ToString()}]";
        }
    }
}
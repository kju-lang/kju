namespace KJU.Core.AST.Types
{
    public class ArrayType : DataType
    {
        public ArrayType(DataType elementType)
        {
            this.ElementType = elementType;
        }

        public DataType ElementType { get; }

        public override string ToString()
        {
            return $"[{this.ElementType.ToString()}]";
        }
    }
}
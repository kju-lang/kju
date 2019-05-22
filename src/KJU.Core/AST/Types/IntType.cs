namespace KJU.Core.AST.BuiltinTypes
{
    public class IntType : DataType
    {
        public static readonly IntType Instance = new IntType();

        public override string ToString()
        {
            return "Int";
        }

        public override bool IsHeapType()
        {
            return false;
        }
    }
}
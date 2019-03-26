namespace KJU.Core.AST.BuiltinTypes
{
    public class BoolType : DataType
    {
        public static readonly BoolType Instance = new BoolType();

        public override string ToString()
        {
            return "Bool";
        }
    }
}
namespace KJU.Core.AST.BuiltinTypes
{
    public class UnitType : DataType
    {
        public static readonly UnitType Instance = new UnitType();

        public override string ToString()
        {
            return "Unit";
        }

        public override bool IsHeapType()
        {
            return false;
        }
    }
}
namespace KJU.Core.AST.Types
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    public class NullType : DataType
    {
        public static readonly NullType Instance = new NullType();

        public override string ToString()
        {
            return "Null";
        }

        public override bool IsHeapType()
        {
            return false;
        }
    }
}
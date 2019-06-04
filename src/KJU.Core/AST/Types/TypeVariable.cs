namespace KJU.Core.AST.Types
{
    using System;
    using Lexer;

    public class TypeVariable : DataType
    {
        public Range InputRange { get; set; }

        public override string ToString()
        {
            return "TypeVariable";
        }

        public override bool IsHeapType()
        {
            throw new ArgumentException("uninstantiated variable");
        }

        public override object GetTag()
        {
            throw new ArgumentException("attempt to use TypeVariable as a normal value");
        }
    }
}

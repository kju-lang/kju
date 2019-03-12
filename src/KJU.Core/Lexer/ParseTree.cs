namespace KJU.Core.Lexer
{
    using System;
    using System.Collections.Generic;

    public abstract class ParseTree<TLabel>
    {
        public TLabel Category { get; set; }

        public Range InputRange { get; set; }
    }
}

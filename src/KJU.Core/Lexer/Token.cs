namespace KJU.Core.Lexer
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using KJU.Core.Input;

    public class Token
    {
        public TokenCategory Category { get; set; }

        public string Text { get; set; }

        public Range InputRange { get; set; }
    }
}

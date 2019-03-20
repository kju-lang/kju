namespace KJU.Core.Lexer
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using KJU.Core.Input;

    public class Token<TLabel> : ParseTree<TLabel>
    {
        public string Text { get; set; }

        public override string ToString()
        {
            return $"{this.Category}'{this.Text}'";
        }
    }
}

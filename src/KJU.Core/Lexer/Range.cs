namespace KJU.Core.Lexer
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using KJU.Core.Input;

    public class Range
    {
        public ILocation Begin { get; set; }

        public ILocation End { get; set; }
    }
}

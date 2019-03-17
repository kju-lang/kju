namespace KJU.Core.Parser
{
    using System;
    using System.Collections.Generic;
    using KJU.Core.Automata;
    using KJU.Core.Input;
    using KJU.Core.Lexer;
    using KJU.Core.Regex;

    public struct ParseAction<TLabel>
    {
        public enum ActionKind
        {
            Shift, Reduce, Call
        }

        public ActionKind Kind { get; set; }

        public TLabel Label { get; set; }

        public override string ToString()
        {
            return $"{this.Kind}: {this.Label}";
        }
    }
}
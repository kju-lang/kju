namespace KJU.Core.Parser
{
    using System;
    using System.Collections.Generic;
    using KJU.Core.Automata;
    using KJU.Core.Input;
    using KJU.Core.Lexer;
    using KJU.Core.Regex;

    public class Rule<TLabel>
    {
        public TLabel Lhs { get; set; }

        public Regex<TLabel> Rhs { get; set; }

        public string Name { get; set; }

        public override string ToString()
        {
            return $"Rule({(this.Name == null ? "unknown" : this.Name)} symbol={this.Lhs})";
        }
    }
}
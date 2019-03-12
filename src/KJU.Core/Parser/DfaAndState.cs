namespace KJU.Core.Parser
{
    using System;
    using System.Collections.Generic;
    using KJU.Core.Automata;
    using KJU.Core.Input;
    using KJU.Core.Lexer;
    using KJU.Core.Regex;

    public struct DfaAndState<TLabel>
    {
        public IDfa<Rule<TLabel>, TLabel> Dfa { get; set; }

        public IState State { get; set; }
    }
}
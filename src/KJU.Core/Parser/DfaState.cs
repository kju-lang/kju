namespace KJU.Core.Parser
{
    using System;
    using System.Collections.Generic;
    using KJU.Core.Automata;
    using KJU.Core.Input;
    using KJU.Core.Lexer;
    using KJU.Core.Regex;

    public struct DfaState<TLabel>
    {
        public IDfa<Rule<TLabel>> Dfa { get; set; } // TODO: IDfa<TLabel, Rule<TLabel>>

        public IState State { get; set; }
    }
}
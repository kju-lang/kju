namespace KJU.Core.Lexer
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using KJU.Core.Automata;
    using KJU.Core.Input;

    public class Lexer
    {
        // tokenCategories - List of pair (Token, Regex for thie Token)
        public Lexer(IEnumerable<KeyValuePair<TokenCategory, string>> tokenCategories)
        {
            throw new NotImplementedException();
        }

        // We assume last element of input is char(-1), must ensure it somewhere
        public IEnumerable<Token> Scan(List<KeyValuePair<ILocation, char>> text)
        {
            throw new NotImplementedException();
        }

        // private readonly IDfa<TokenCategory> minimalizedDfa;
    }
}

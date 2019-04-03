namespace KJU.Core.Parser
{
    using Lexer;

    public static class KjuParserFactory
    {
        public static Parser<KjuAlphabet> CreateParser()
        {
            return ParserFactory<KjuAlphabet>.MakeParser(KjuGrammar.Instance, KjuAlphabet.Eof);
        }
    }
}
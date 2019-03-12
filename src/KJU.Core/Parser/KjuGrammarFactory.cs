namespace KJU.Core.Parser
{
    public class KjuGrammarFactory : IGrammarFactory<KjuAlphabet>
    {
        public Grammar<KjuAlphabet> Create()
        {
            // var kju = new Rule<KjuAlphabet>(new StarRegex(KjuAlphabet.Function));
            // var function=new Rule<KjuAlphabet>{Lhs = KjuAlphabet.Function, Rhs = new ConcatRegex()}
            return new Grammar<KjuAlphabet> { StartSymbol = KjuAlphabet.Kju };
        }
    }
}
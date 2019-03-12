namespace KJU.Core.Parser
{
    public interface IGrammarFactory<TLabel>
    {
        Grammar<TLabel> Create();
    }
}
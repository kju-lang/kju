namespace KJU.Application
{
    using KJU.Core.Input;

    public class CompilationQuery
    {
        public CompilationQuery(IInputReader input, string resultPath)
        {
            this.Input = input;
            this.ResultPath = resultPath;
        }

        public IInputReader Input { get; }

        public string ResultPath { get; }
    }
}
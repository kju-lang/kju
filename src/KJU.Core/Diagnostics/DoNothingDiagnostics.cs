namespace KJU.Core.Diagnostics
{
    public class DoNothingDiagnostics : IDiagnostics
    {
        public void Add(params Diagnostic[] diagnostics)
        {
        }

        public void Report()
        {
        }
    }
}
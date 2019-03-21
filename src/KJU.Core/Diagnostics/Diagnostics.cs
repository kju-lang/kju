namespace KJU.Core.Diagnostics
{
    using System.Collections.Generic;

    public class Diagnostics : IDiagnostics
    {
        public List<Diagnostic> Diags { get; } = new List<Diagnostic>();

        public void Add(params Diagnostic[] diagnostics)
        {
            this.Diags.AddRange(diagnostics);
        }

        public void Report()
        {
            return;
        }
    }
}
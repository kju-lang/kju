namespace KJU.Core.Diagnostics
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using KJU.Core.Diagnostics;

    public class TextWriterDiagnostics : IDiagnostics
    {
        private static Dictionary<DiagnosticStatus, string> statusPrefix = new Dictionary<DiagnosticStatus, string>
        {
            { DiagnosticStatus.Error, "Error" },
            { DiagnosticStatus.Warning, "Warning" },
        };

        private TextWriter writer;

        private List<Diagnostic> diagnostics = new List<Diagnostic>();

        public TextWriterDiagnostics(TextWriter writer)
        {
            this.writer = writer;
        }

        public void Add(params Diagnostic[] ds)
        {
            this.diagnostics.AddRange(ds);
        }

        public void Report()
        {
            foreach (Diagnostic diag in this.diagnostics)
            {
                this.writer.WriteLine(this.FormatMessage(diag));
            }
        }

        private string FormatMessage(Diagnostic diag)
        {
            string formattedMessage = string.Format(diag.Message, diag.Ranges.ToArray());
            return $"{statusPrefix[diag.Status]} ({diag.Type}): {formattedMessage}";
        }
    }
}

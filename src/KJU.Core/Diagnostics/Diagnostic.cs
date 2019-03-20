namespace KJU.Core.Diagnostics
{
    using System.Collections.Generic;
    using KJU.Core.Lexer;

    public class Diagnostic
    {
        public Diagnostic(DiagnosticStatus status, string type, string message, IReadOnlyList<Range> ranges)
        {
            this.Status = status;
            this.Type = type;
            this.Message = message;
            this.Ranges = ranges;
        }

        public DiagnosticStatus Status { get; }

        public string Type { get; }

        public string Message { get; }

        public IReadOnlyList<Range> Ranges { get; }

        public override string ToString()
        {
            // TODO message: String < -format "Error is here {0} and here {1} and here {2}"
            return this.Message;
        }
    }
}
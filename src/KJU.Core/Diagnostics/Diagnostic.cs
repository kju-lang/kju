namespace KJU.Core.Diagnostics
{
    using System.Collections.Generic;
    using System.Linq;
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

        public static string EscapeForMessage(string s)
        {
            return s.Replace("{", "{{").Replace("}", "}}");
        }

        public override string ToString()
        {
            string message = this.Message + " " + string.Join(" ; ", this.Ranges.Select(x => x.ToString()).ToArray());
            return $"{this.Type}: {message}";
        }
    }
}

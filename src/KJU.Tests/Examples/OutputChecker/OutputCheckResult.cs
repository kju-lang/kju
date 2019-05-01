namespace KJU.Tests.Examples.OutputChecker
{
    using System.Collections.Generic;

    public abstract class OutputCheckResult
    {
        private OutputCheckResult()
        {
        }

        public class Correct : OutputCheckResult
        {
            public Correct(List<string> notes = null)
            {
                this.Notes = notes ?? new List<string>();
            }

            public List<string> Notes { get; }
        }

        public class Wrong : OutputCheckResult
        {
            public Wrong(List<string> notes = null)
            {
                this.Notes = notes ?? new List<string>();
            }

            public List<string> Notes { get; }
        }
    }
}
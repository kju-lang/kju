namespace KJU.Tests.Examples.OutputChecker
{
    using System.Collections.Generic;
    using System.Linq;

    public class ExactOutputChecker : IOutputChecker
    {
        private readonly string expectedOutput;
        private readonly CompareMode compareMode;

        public ExactOutputChecker(string expectedOutput, CompareMode compareMode = CompareMode.Normalize)
        {
            this.expectedOutput = expectedOutput;
            this.compareMode = compareMode;
        }

        public enum CompareMode
        {
            Normalize,
            Exact
        }

        public OutputCheckResult CheckOutput(string actualOutput)
        {
            var expected = this.Lines(this.expectedOutput);

            var actual = this.Lines(actualOutput);
            var diffs = new List<string>();

            var expectedLength = expected.Count;
            var actualLength = actual.Count;

            if (expectedLength != actualLength)
            {
                diffs.Add($"Outputs differ in number of lines. Expected: {expectedLength}, actual: {actualLength}");
            }

            var comparisonNotes = expected.Zip(actual, (x, y) => new { Expected = x, Actual = y })
                .Select((x, index) => new { Index = index, x.Expected, x.Actual })
                .Where(x => !x.Expected.Equals(x.Actual))
                .Select(x => $"Outputs differ in line {x.Index}. Expected: {x.Expected}, actual: {x.Actual}");

            diffs.AddRange(comparisonNotes);

            return diffs.Any()
                ? (OutputCheckResult)new OutputCheckResult.Wrong(diffs)
                : (OutputCheckResult)new OutputCheckResult.Correct();
        }

        private static string FixWhitespaces(string input)
        {
            return System.Text.RegularExpressions.Regex.Replace(input, @"\s+", @" ").Trim();
        }

        private IList<string> Lines(string input)
        {
            var lines = input.Split("\n");
            return this.compareMode == CompareMode.Normalize
                ? lines.Select(FixWhitespaces).Where(x => !x.Equals(string.Empty)).ToList()
                : lines.ToList();
        }
    }
}
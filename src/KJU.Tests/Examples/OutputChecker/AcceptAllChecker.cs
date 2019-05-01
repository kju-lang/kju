namespace KJU.Tests.Examples.OutputChecker
{
    public class AcceptAllChecker : IOutputChecker
    {
        public OutputCheckResult CheckOutput(string actualOutput)
        {
            return new OutputCheckResult.Correct();
        }
    }
}
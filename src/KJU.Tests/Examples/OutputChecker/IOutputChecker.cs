namespace KJU.Tests.Examples.OutputChecker
{
    using System.Collections.Generic;

    public interface IOutputChecker
    {
        OutputCheckResult CheckOutput(string actualOutput);
    }
}
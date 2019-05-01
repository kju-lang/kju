namespace KJU.Tests.Examples
{
    using System.Collections.Generic;
    using KJU.Core.Input;
    using KJU.Tests.Examples.OutputChecker;

    public interface IKjuExample
    {
        IInputReader Program { get; }

        string Name { get; }

        bool IsPositive { get; }

        bool IsDisabled { get; }

        string Input { get; }

        bool Ends { get; }

        long Timeout { get; }

        IOutputChecker OutputChecker { get; }

        IEnumerable<string> ExpectedMagicStrings { get; }
    }
}
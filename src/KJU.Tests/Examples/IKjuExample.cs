namespace KJU.Tests.Examples
{
    using System.Collections.Generic;
    using KJU.Core.Input;
    using KJU.Tests.Examples.OutputChecker;

    public interface IKjuExample
    {
        IInputReader Program { get; }

        string Name { get; }

        string SimpleName { get; }

        bool IsPositive { get; }

        bool IsDisabled { get; }

        bool Executable { get; }

        string Input { get; }

        bool Ends { get; }

        int Timeout { get; }

        IOutputChecker OutputChecker { get; }

        IEnumerable<string> ExpectedMagicStrings { get; }
    }
}
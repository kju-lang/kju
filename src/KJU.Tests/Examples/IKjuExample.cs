namespace KJU.Tests.Examples
{
    using System.Collections.Generic;
    using KJU.Core.Input;

    public interface IKjuExample
    {
        IInputReader Program { get; }

        string Name { get; }

        bool IsPositive { get; }

        bool IsDisabled { get; }

        string Input { get; }

        bool Ends { get; }

        string ExpectedOutput { get; }

        IEnumerable<string> ExpectedMagicStrings { get; }
    }
}
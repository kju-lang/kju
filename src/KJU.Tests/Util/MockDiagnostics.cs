namespace KJU.Tests.Util
{
    using System.Collections.Generic;
    using System.Linq;
    using Core.Diagnostics;
    using Moq;

    public static class MockDiagnostics
    {
        public static void Verify(Mock<IDiagnostics> diag, params string[] diagTypes)
        {
            var toCheck = diagTypes.GroupBy(x => x, (key, list) => new KeyValuePair<string, int>(key, list.Count()));
            foreach (var type in toCheck)
            {
                diag.Verify(
                    ds => ds.Add(It.Is<Diagnostic>(d => d.Type == type.Key)),
                    () => Times.Exactly(type.Value),
                    $"Expected a Diagnostic with Type \"{type}\"");
            }

            diag.VerifyNoOtherCalls();
        }
    }
}

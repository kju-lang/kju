namespace KJU.Tests.Util
{
    using KJU.Core.Diagnostics;
    using Moq;

    public class MockDiagnostics
    {
        public static void Verify(Mock<IDiagnostics> diag, params string[] diagTypes)
        {
            foreach (string type in diagTypes)
            {
                diag.Verify(
                    ds => ds.Add(It.Is<Diagnostic>(d => d.Type == type)),
                    $"Expected a Diagnostic with Type \"{type}\"");
            }

            diag.VerifyNoOtherCalls();
        }
    }
}

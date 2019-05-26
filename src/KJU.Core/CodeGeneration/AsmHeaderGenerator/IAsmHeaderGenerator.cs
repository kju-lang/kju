namespace KJU.Core.CodeGeneration.AsmHeaderGenerator
{
    public interface IAsmHeaderGenerator
    {
        string GenerateHeader();

        string GenerateDataSectionHeader();
    }
}
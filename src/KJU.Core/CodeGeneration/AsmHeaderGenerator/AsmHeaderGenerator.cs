namespace KJU.Core.CodeGeneration.AsmHeaderGenerator
{
    public class AsmHeaderGenerator : IAsmHeaderGenerator
    {
        public string GenerateHeader()
        {
            return @"global main
section .text
main:
jmp _ZN3KJU3kjuEv
";
        }
    }
}
namespace KJU.Core.CodeGeneration.AsmHeaderGenerator
{
    public class AsmHeaderGenerator : IAsmHeaderGenerator
    {
        public string GenerateHeader()
        {
            return @"global main
section .text
main:
push rbp
xor rbp, rbp
cld
call _ZN3KJU3kjuEv
pop rbp
ret
";
        }

        public string GenerateDataSectionHeader()
        {
            return "section .data";
        }
    }
}
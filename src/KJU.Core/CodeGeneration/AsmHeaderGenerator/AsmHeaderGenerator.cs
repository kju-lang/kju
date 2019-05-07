namespace KJU.Core.CodeGeneration.AsmHeaderGenerator
{
    public class AsmHeaderGenerator : IAsmHeaderGenerator
    {
        public string GenerateHeader()
        {
            return @"global main
section .text
main:
call _ZN3KJU3kjuEv
xor RAX, RAX
ret
";
        }
    }
}
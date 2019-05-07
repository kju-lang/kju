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
call _ZN3KJU3kjuEv
pop rbp
xor RAX, RAX
ret
";
        }
    }
}
namespace KJU.Core.CodeGeneration.AsmHeaderGenerator
{
    public class AsmHeaderGenerator : IAsmHeaderGenerator
    {
        public string GenerateHeader()
        {
            return @"global _start
section .text
_start:
call _ZN3KJU3kjuEv
mov RAX, 60       
mov RDI, 0       
syscall          
";
        }
    }
}
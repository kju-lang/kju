namespace KJU.Core.Compiler
{
    using System.Collections.Generic;

    public class Artifacts
    {
        public KJU.Core.AST.Node Ast { get; set; }

        public IEnumerable<string> Asm { get; set; }
    }
}

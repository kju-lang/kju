namespace KJU.Core.Compiler
{
    using System.Collections.Generic;
    using AST;

    public class Artifacts
    {
        public Artifacts(Node ast, IEnumerable<string> asm)
        {
            this.Ast = ast;
            this.Asm = asm;
        }

        public Node Ast { get; }

        public IEnumerable<string> Asm { get; }
    }
}
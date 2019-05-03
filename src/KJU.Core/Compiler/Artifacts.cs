namespace KJU.Core.Compiler
{
    using System.Collections.Generic;
    using AST;

    public class Artifacts
    {
        public Artifacts(Node ast, string asm)
        {
            this.Ast = ast;
            this.Asm = asm;
        }

        public Node Ast { get; }

        public string Asm { get; }
    }
}
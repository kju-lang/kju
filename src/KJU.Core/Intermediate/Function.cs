#pragma warning disable CS0169
namespace KJU.Core.Intermediate
{
    using System;
    using System.Collections.Generic;

    public class Function
    {
        private Variable link;
        private List<Variable> arguments;
        private Function parent;

        public MemoryLocation ReserveStackFrameLocation()
        {
            throw new NotImplementedException();
        }

        public Label GenerateCall(VirtualRegister result, List<VirtualRegister> args, Label onReturn, Function caller)
        {
            throw new NotImplementedException();
        }

        public Tree GenerateRead(Variable v, Tree framePointer)
        {
            throw new NotImplementedException();
        }

        public Tree GenerateWrite(Variable v, Tree value, Tree framePointer)
        {
            throw new NotImplementedException();
        }

        public Label GeneratePrologue(Label after)
        {
            throw new NotImplementedException();
        }

        public Label GenerateEpilogue(Tree retVal, Label after)
        {
            throw new NotImplementedException();
        }

        public Label GenerateBody(Label after, AST.FunctionDeclaration root)
        {
            throw new NotImplementedException();
        }
    }
}
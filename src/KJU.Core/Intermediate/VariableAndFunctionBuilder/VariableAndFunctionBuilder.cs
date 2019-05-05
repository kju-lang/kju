namespace KJU.Core.Intermediate.VariableAndFunctionBuilder
{
    using System.Linq;

    public class VariableAndFunctionBuilder : IVariableAndFunctionBuilder
    {
        private readonly NameMangler.NameMangler nameMangler;

        public VariableAndFunctionBuilder(NameMangler.NameMangler nameMangler)
        {
            this.nameMangler = nameMangler;
        }

        public void BuildFunctionsAndVariables(AST.Node root)
        {
            this.TraverseTree(root);
        }

        private void TraverseTree(AST.Node node, Function.Function parent = null)
        {
            Function.Function newParent;

            switch (node)
            {
                case AST.FunctionDeclaration functionDeclaration:
                {
                    var mangledName = this.nameMangler.GetMangledName(functionDeclaration, parent?.MangledName);
                    var function = new Function.Function(parent, mangledName, functionDeclaration.Parameters);
                    functionDeclaration.IntermediateFunction = function;
                    newParent = function;
                    break;
                }

                case AST.VariableDeclaration variableDeclaration:
                {
                    variableDeclaration.IntermediateVariable = new Variable(parent, parent.ReserveStackFrameLocation());
                    newParent = parent;
                    break;
                }

                default:
                {
                    newParent = parent;
                    break;
                }
            }

            foreach (var child in node.Children())
            {
                this.TraverseTree(child, newParent);
            }
        }
    }
}
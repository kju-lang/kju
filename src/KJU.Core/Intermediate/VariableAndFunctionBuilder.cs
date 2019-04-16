namespace KJU.Core.Intermediate
{
    using System.Linq;
    using AST;

    public static class VariableAndFunctionBuilder
    {
        public static void BuildFunctionsAndVariables(AST.Node root)
        {
            TraverseTree(root, null);
        }

        private static void TraverseTree(AST.Node node, Function parent)
        {
            var newParent = parent;

            switch (node)
            {
                case FunctionDeclaration functionDeclaration:
                {
                    var function = new Function(parent);
                    function.MangledName = NameMangler.GetMangledName(functionDeclaration, parent == null ? null : parent.MangledName);
                    function.Link = new Variable(function, function.ReserveStackFrameLocation());
                    function.Arguments = functionDeclaration.Parameters.Select(param =>
                    {
                        var location = function.ReserveStackFrameLocation();
                        var variable = new Variable(function, location);
                        param.IntermediateVariable = variable;
                        return variable;
                    }).ToList();

                    functionDeclaration.IntermediateFunction = function;
                    newParent = function;
                    break;
                }

                case VariableDeclaration variableDeclaration:
                {
                    variableDeclaration.IntermediateVariable = new Variable(parent, parent.ReserveStackFrameLocation());
                    break;
                }
            }

            foreach (var child in node.Children())
            {
                TraverseTree(child, newParent);
            }
        }
    }
}
namespace KJU.Core.Intermediate.VariableAndFunctionBuilder
{
    using System.Collections.Generic;
    using Function;

    public class VariableAndFunctionBuilder : IVariableAndFunctionBuilder
    {
        private readonly FunctionBuilder functionBuilder;

        public VariableAndFunctionBuilder(FunctionBuilder functionBuilder)
        {
            this.functionBuilder = functionBuilder;
        }

        public void BuildFunctionsAndVariables(AST.Node root)
        {
            var variableUsages = new Dictionary<AST.VariableDeclaration, HashSet<AST.FunctionDeclaration>>();
            this.BuildVariableUsages(root, null, variableUsages);
            this.TraverseTree(root, variableUsages);
        }

        private void BuildVariableUsages(
            AST.Node node,
            AST.FunctionDeclaration parent,
            IDictionary<AST.VariableDeclaration, HashSet<AST.FunctionDeclaration>> result)
        {
            var newParent = parent;

            switch (node)
            {
                case AST.FunctionDeclaration declaration:
                {
                    newParent = declaration;
                    break;
                }

                case AST.VariableDeclaration declaration:
                {
                    if (!result.ContainsKey(declaration))
                    {
                        result.Add(declaration, new HashSet<AST.FunctionDeclaration>());
                    }

                    result[declaration].Add(parent);
                    break;
                }

                case AST.Variable variable:
                {
                    if (!result.ContainsKey(variable.Declaration))
                    {
                        result.Add(variable.Declaration, new HashSet<AST.FunctionDeclaration>());
                    }

                    result[variable.Declaration].Add(parent);
                    break;
                }
            }

            foreach (var child in node.Children())
            {
                this.BuildVariableUsages(child, newParent, result);
            }
        }

        private void TraverseTree(
            AST.Node node,
            IReadOnlyDictionary<AST.VariableDeclaration, HashSet<AST.FunctionDeclaration>> variableUsages,
            Function parentFunction = null)
        {
            switch (node)
            {
                case AST.FunctionDeclaration functionDeclaration:
                {
                    var function = this.functionBuilder.CreateFunction(functionDeclaration, parentFunction);
                    functionDeclaration.Function = function;
                    foreach (var argument in functionDeclaration.Parameters)
                    {
                        argument.IntermediateVariable = variableUsages[argument].Count > 1
                            ? (ILocation)function.ReserveStackFrameLocation(argument.VariableType)
                            : new VirtualRegister();
                    }

                    if (functionDeclaration.Body != null)
                    {
                        this.TraverseTree(functionDeclaration.Body, variableUsages, function);
                    }

                    return;
                }

                case AST.VariableDeclaration variableDeclaration:
                {
                    variableDeclaration.IntermediateVariable = parentFunction.ReserveStackFrameLocation(variableDeclaration.VariableType);
                    break;
                }
            }

            foreach (var child in node.Children())
            {
                this.TraverseTree(child, variableUsages, parentFunction);
            }
        }
    }
}
namespace KJU.Core.Intermediate.VariableAndFunctionBuilder
{
    using System.Collections.Generic;
    using Function;

    public class VariableAndFunctionBuilder : IVariableAndFunctionBuilder
    {
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

        private ILocation ReserveLocation(Function function, IReadOnlyDictionary<AST.VariableDeclaration, HashSet<AST.FunctionDeclaration>> variableUsages, AST.VariableDeclaration variable)
        {
            if (variableUsages[variable].Count > 1)
                return function.ReserveClosureLocation(variable.Identifier, variable.VariableType);
            else if (variable.VariableType.IsHeapType())
                return function.ReserveStackFrameLocation(variable.VariableType);
            else
                return new VirtualRegister();
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
                    var function = FunctionBuilder.CreateFunction(functionDeclaration, parentFunction);
                    functionDeclaration.Function = function;
                    foreach (var argument in functionDeclaration.Parameters)
                    {
                        argument.IntermediateVariable = this.ReserveLocation(function, variableUsages, argument);
                    }

                    if (functionDeclaration.Body != null)
                    {
                        this.TraverseTree(functionDeclaration.Body, variableUsages, function);
                    }

                    return;
                }

                case AST.VariableDeclaration variableDeclaration:
                {
                    variableDeclaration.IntermediateVariable = this.ReserveLocation(parentFunction, variableUsages, variableDeclaration);
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

namespace KJU.Core.Intermediate.VariableAndFunctionBuilder
{
    using System;
    using System.Collections.Generic;
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
            Dictionary<AST.VariableDeclaration, HashSet<AST.FunctionDeclaration>> variableUsages = new Dictionary<AST.VariableDeclaration, HashSet<AST.FunctionDeclaration>>();
            this.BuildVariableUsages(root, null, variableUsages);

            this.TraverseTree(root, variableUsages);
        }

        private void BuildVariableUsages(
            AST.Node node,
            AST.FunctionDeclaration parent,
            Dictionary<AST.VariableDeclaration, HashSet<AST.FunctionDeclaration>> result)
        {
            AST.FunctionDeclaration newParent = parent;

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

        private void VisitArguments(
            IReadOnlyList<AST.VariableDeclaration> arguments,
            Dictionary<AST.VariableDeclaration, HashSet<AST.FunctionDeclaration>> variableUsages,
            Function.Function parent)
        {
            for (int i = 0; i < arguments.Count; i++)
            {
                if (variableUsages[arguments[i]].Count > 1)
                {
                    arguments[i].IntermediateVariable = new Variable(parent, parent.ReserveStackFrameLocation());
                }
                else
                {
                    arguments[i].IntermediateVariable = new Variable(parent, new VirtualRegister());
                }
            }
        }

        private void TraverseTree(AST.Node node, Dictionary<AST.VariableDeclaration, HashSet<AST.FunctionDeclaration>> variableUsages, Function.Function parent = null)
        {
            Function.Function newParent;

            switch (node)
            {
                case AST.FunctionDeclaration functionDeclaration:
                    {
                        var mangledName = this.nameMangler.GetMangledName(functionDeclaration, parent?.MangledName);
                        var function = new Function.Function(parent, mangledName, functionDeclaration.Parameters, isForeign: functionDeclaration.IsForeign);
                        functionDeclaration.IntermediateFunction = function;

                        this.VisitArguments(functionDeclaration.Parameters, variableUsages, function);
                        if (functionDeclaration.Body != null)
                        {
                            this.TraverseTree(functionDeclaration.Body, variableUsages, function);
                        }

                        return;
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
                this.TraverseTree(child, variableUsages, newParent);
            }
        }
    }
}
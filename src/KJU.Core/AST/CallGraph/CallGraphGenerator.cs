namespace KJU.Core.AST.CallGraph
{
    using System.Collections.Generic;

    public class CallGraphGenerator : ICallGraphGenerator
    {
        private Dictionary<FunctionDeclaration, IReadOnlyCollection<FunctionDeclaration>> callGraph;

        public IReadOnlyDictionary<FunctionDeclaration, IReadOnlyCollection<FunctionDeclaration>> BuildCallGraph(Node root)
        {
            this.callGraph = new Dictionary<FunctionDeclaration, IReadOnlyCollection<FunctionDeclaration>>();
            this.ProcessNode(root);
            return this.callGraph;
        }

        private IReadOnlyCollection<FunctionDeclaration> ProcessNode(Node node)
        {
            HashSet<FunctionDeclaration> funs = new HashSet<FunctionDeclaration>();

            switch (node)
            {
                case FunctionDeclaration fun:
                    this.callGraph.Add(fun, this.ProcessNode(fun.Body));
                    break;
                case FunctionCall call:
                    foreach (var child in call.Children())
                    {
                        funs.UnionWith(this.ProcessNode(child));
                    }

                    funs.Add(call.Declaration);
                    break;
                default:
                    foreach (var child in node.Children())
                    {
                        funs.UnionWith(this.ProcessNode(child));
                    }

                    break;
            }

            return funs;
        }
    }
}
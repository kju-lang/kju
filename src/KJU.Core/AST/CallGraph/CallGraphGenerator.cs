namespace KJU.Core.AST.CallGraph
{
    using System.Collections.Generic;
    using System.Linq;

    public class CallGraphGenerator : ICallGraphGenerator
    {
        private Dictionary<FunctionDeclaration, IReadOnlyCollection<FunctionDeclaration>> callGraph;

        public IReadOnlyDictionary<FunctionDeclaration, IReadOnlyCollection<FunctionDeclaration>> BuildCallGraph(
            Node root)
        {
            this.callGraph = new Dictionary<FunctionDeclaration, IReadOnlyCollection<FunctionDeclaration>>();
            this.ProcessNode(root);
            return this.callGraph
                .Select(kvp => new { Function = kvp.Key, CallSite = kvp.Value.Where(this.callGraph.ContainsKey).ToList() })
                .ToDictionary(x => x.Function, x => (IReadOnlyCollection<FunctionDeclaration>)x.CallSite);
        }

        private HashSet<FunctionDeclaration> ProcessNode(Node node)
        {
            var functions = new HashSet<FunctionDeclaration>();
            switch (node)
            {
                case FunctionDeclaration fun:
                    this.callGraph.Add(fun, this.ProcessNode(fun.Body));
                    break;
                case FunctionCall call:
                    foreach (var child in call.Children())
                    {
                        functions.UnionWith(this.ProcessNode(child));
                    }

                    functions.Add(call.Declaration);
                    break;
                default:
                    foreach (var child in node.Children())
                    {
                        functions.UnionWith(this.ProcessNode(child));
                    }

                    break;
            }

            return functions;
        }
    }
}
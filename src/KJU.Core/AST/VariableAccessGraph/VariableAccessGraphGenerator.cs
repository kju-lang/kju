namespace KJU.Core.AST.VariableAccessGraph
{
    using System.Collections.Generic;
    using System.Linq;
    using KJU.Core.AST.CallGraph;
    using Util;
    using FunctionVariableAccessMapping =
        System.Collections.Generic.IReadOnlyDictionary<FunctionDeclaration,
            System.Collections.Generic.IReadOnlyCollection<VariableDeclaration>>;
    using NodeVariableAccessMapping =
        System.Collections.Generic.IReadOnlyDictionary<Node,
            System.Collections.Generic.IReadOnlyCollection<VariableDeclaration>>;

    public class VariableAccessGraphGenerator
    {
        private readonly ICallGraphGenerator callGraphGenerator;

        public VariableAccessGraphGenerator(ICallGraphGenerator callGraphGenerator)
        {
            this.callGraphGenerator = callGraphGenerator;
        }

        private interface INodeInfoExtractor
        {
            IEnumerable<VariableDeclaration> ExtractInfo(Node node);
        }

        public FunctionVariableAccessMapping
            BuildVariableAccessGraph(Node root)
        {
            var variablesExtractor = new AccessInfoExtractor();
            return this.TransitiveCallClosure(root, variablesExtractor);
        }

        public FunctionVariableAccessMapping
            BuildVariableModificationGraph(Node root)
        {
            var variablesExtractor = new ModifyInfoExtractor();
            return this.TransitiveCallClosure(root, variablesExtractor);
        }

        public NodeVariableAccessMapping BuildVariableAccessesPerAstNode(
            Node root,
            FunctionVariableAccessMapping accessGraph)
        {
            var extractor = new AccessInfoExtractor();
            return this.AggregateNodeVariableInfo(root, accessGraph, extractor);
        }

        public NodeVariableAccessMapping BuildVariableModificationsPerAstNode(
            Node root,
            FunctionVariableAccessMapping modificationGraph)
        {
            var extractor = new ModifyInfoExtractor();
            return this.AggregateNodeVariableInfo(root, modificationGraph, extractor);
        }

        private FunctionVariableAccessMapping
            TransitiveCallClosure(
                Node root,
                INodeInfoExtractor variablesExtractor)
        {
            var callGraphClosure = TransitiveClosure<FunctionDeclaration>.ComputeTransitiveClosure(this.callGraphGenerator.BuildCallGraph(root));
            var functions = this.AggregateFunctionDeclarations(root).ToList();
            return functions.ToDictionary(fun => fun, function =>
            {
                var result = this.AggregateFunctionVariables(function, variablesExtractor);
                callGraphClosure[function]
                    .Select(foo => this.AggregateFunctionVariables(foo, variablesExtractor))
                    .ToList()
                    .ForEach(x => result.UnionWith(x));
                return (IReadOnlyCollection<VariableDeclaration>)result;
            });
        }

        private IEnumerable<FunctionDeclaration> AggregateFunctionDeclarations(Node root)
        {
            var result = root.Children().SelectMany(this.AggregateFunctionDeclarations);
            if (root is FunctionDeclaration functionDeclaration)
            {
                return result.Append(functionDeclaration);
            }

            return result;
        }

        private NodeVariableAccessMapping AggregateNodeVariableInfo(
            Node root,
            FunctionVariableAccessMapping infoPerFunction,
            INodeInfoExtractor infoExtractor)
        {
            var dictionary = root.Children()
                .SelectMany(child => this.AggregateNodeVariableInfo(child, infoPerFunction, infoExtractor))
                .ToDictionary(x => x.Key, x => x.Value);

            var rootSet = new HashSet<VariableDeclaration>(infoExtractor.ExtractInfo(root));

            if (!(root is FunctionDeclaration))
            {
                root.Children().ToList().ForEach(child => rootSet.UnionWith(dictionary[child]));
            }

            if (root is FunctionCall call)
            {
                rootSet.UnionWith(infoPerFunction[call.Declaration]);
            }

            dictionary[root] = rootSet;
            return dictionary;
        }

        private HashSet<VariableDeclaration> AggregateFunctionVariables(
            FunctionDeclaration functionDeclaration,
            INodeInfoExtractor infoExtractor)
        {
            var variables = new HashSet<VariableDeclaration>(functionDeclaration.Parameters);
            var queue = new Queue<Node>();
            queue.Enqueue(functionDeclaration.Body);
            while (queue.Count > 0)
            {
                var node = queue.Dequeue();
                if (node is FunctionDeclaration)
                {
                    continue;
                }

                variables.UnionWith(infoExtractor.ExtractInfo(node));
                foreach (var child in node.Children())
                {
                    queue.Enqueue(child);
                }
            }

            return variables;
        }

        private class AccessInfoExtractor : INodeInfoExtractor
        {
            public IEnumerable<VariableDeclaration> ExtractInfo(Node node)
            {
                switch (node)
                {
                    case Variable variable:
                        return new List<VariableDeclaration>() { variable.Declaration };

                    default:
                        return new List<VariableDeclaration>();
                }
            }
        }

        private class ModifyInfoExtractor : INodeInfoExtractor
        {
            public IEnumerable<VariableDeclaration> ExtractInfo(Node node)
            {
                switch (node)
                {
                    case Assignment assignment:
                        return new List<VariableDeclaration>() { assignment.Lhs.Declaration };

                    case CompoundAssignment compoundAssignment:
                        return new List<VariableDeclaration>() { compoundAssignment.Lhs.Declaration };

                    default:
                        return new List<VariableDeclaration>();
                }
            }
        }
    }
}
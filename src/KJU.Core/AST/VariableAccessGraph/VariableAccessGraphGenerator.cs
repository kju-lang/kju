namespace KJU.Core.AST.VariableAccessGraph
{
    using System;
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

    public class VariableAccessGraphGenerator : IVariableAccessGraphGenerator
    {
        private readonly ICallGraphGenerator callGraphGenerator;
        private readonly Dictionary<VariableInfo, INodeInfoExtractor> infoExtractors;

        public VariableAccessGraphGenerator(
            ICallGraphGenerator callGraphGenerator,
            Dictionary<VariableInfo, INodeInfoExtractor> infoExtractors)
        {
            this.callGraphGenerator = callGraphGenerator;
            this.infoExtractors = infoExtractors;
        }

        // TODO make private as is only used in tests
        public static FunctionVariableAccessMapping
            TransitiveCallClosure(
                ICallGraphGenerator callGraphGenerator,
                Node root,
                INodeInfoExtractor variablesExtractor)
        {
            var callGraph = callGraphGenerator.BuildCallGraph(root);
            var callGraphClosure = callGraph.TransitiveClosure();
            return AggregateFunctionDeclarations(root).ToDictionary(fun => fun, function =>
            {
                var rootVariables = AggregateFunctionVariablesInfo(function, variablesExtractor);
                var closureVariables = callGraphClosure[function]
                    .SelectMany(foo => AggregateFunctionVariablesInfo(foo, variablesExtractor));
                return (IReadOnlyCollection<VariableDeclaration>)rootVariables.Concat(closureVariables).ToList();
            });
        }

        public NodeVariableAccessMapping GetVariableInfoPerAstNode(Node root, VariableInfo variableInfo)
        {
            var extractor = this.infoExtractors[variableInfo];
            var accessGraph = TransitiveCallClosure(this.callGraphGenerator, root, extractor);
            return AggregateNodeVariableInfo(root, accessGraph, extractor);
        }

        private static IEnumerable<VariableDeclaration> AggregateFunctionVariablesInfo(
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

        private static IEnumerable<FunctionDeclaration> AggregateFunctionDeclarations(Node root)
        {
            var result = root.Children().SelectMany(AggregateFunctionDeclarations);

            return root is FunctionDeclaration functionDeclaration ? result.Append(functionDeclaration) : result;
        }

        private static NodeVariableAccessMapping AggregateNodeVariableInfo(
            Node root,
            FunctionVariableAccessMapping infoPerFunction,
            INodeInfoExtractor infoExtractor)
        {
            var result = root.Children()
                .SelectMany(child => AggregateNodeVariableInfo(child, infoPerFunction, infoExtractor))
                .ToDictionary(x => x.Key, x => x.Value);

            var descendantsDeclarations = root is FunctionDeclaration
                ? new List<VariableDeclaration>()
                : root.Children().SelectMany(child => result[child]);

            var callDeclarations = root is FunctionCall call
                ? infoPerFunction[call.Declaration]
                : new List<VariableDeclaration>();

            result[root] = new HashSet<VariableDeclaration>(
                infoExtractor.ExtractInfo(root)
                    .Concat(descendantsDeclarations)
                    .Concat(callDeclarations));
            return result;
        }
    }
}
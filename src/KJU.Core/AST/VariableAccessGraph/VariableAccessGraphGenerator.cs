namespace KJU.Core.AST.VariableAccessGraph
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using FunctionVariableAccessMapping =
        System.Collections.Generic.IReadOnlyDictionary<FunctionDeclaration,
            System.Collections.Generic.IReadOnlyCollection<VariableDeclaration>>;
    using NodeVariableAccessMapping =
        System.Collections.Generic.IReadOnlyDictionary<Node,
            System.Collections.Generic.IReadOnlyCollection<VariableDeclaration>>;

    public class VariableAccessGraphGenerator
    {
        private interface INodeInfoExtractor
        {
            IEnumerable<VariableDeclaration> ExtractInfo(Node node);
        }

        public FunctionVariableAccessMapping BuildVariableAccessGraph(Node root)
        {
            throw new NotImplementedException();
        }

        public FunctionVariableAccessMapping BuildVariableModificationGraph(Node root)
        {
            throw new NotImplementedException();
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
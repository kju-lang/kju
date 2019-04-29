namespace KJU.Core.Visualization
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Runtime.Serialization;
    using System.Text;
    using System.Text.RegularExpressions;
    using KJU.Core.AST;
    using KJU.Core.Util;

    public class AstToDotConverter
    {
        public static IEnumerable<string> Convert(Node node)
        {
            var nodes = new List<string>();
            var edges = new List<string>();

            var objectIdGenerator = new ObjectIDGenerator();

            var header = GenHeader();
            var footer = GenFooter();

            ProcessNode(node, nodes, objectIdGenerator);
            ProcessNodeEdges(node, edges, objectIdGenerator);

            return header
                .Concat(nodes)
                .Concat(edges)
                .Concat(footer);
        }

        private static IEnumerable<string> GenHeader()
        {
            return new string[] { "digraph {" };
        }

        private static IEnumerable<string> GenFooter()
        {
            return new string[] { "}" };
        }

        private static void ProcessNode(Node node, List<string> nodes, ObjectIDGenerator objectIdGenerator)
        {
            long id = objectIdGenerator.GetId(node, out var firstTime);

            Debug.Assert(firstTime, "AST should not have cycles");

            switch (node)
            {
                default:
                    nodes.Add($"  {id} [label=\"{Dot.Escape(node.ToString())}\", shape=box];");

                    foreach (var child in node.Children())
                    {
                        ProcessNode(child, nodes, objectIdGenerator);
                    }

                    break;
            }
        }

        private static void ProcessNodeEdges(Node node, List<string> edges, ObjectIDGenerator objectIdGenerator)
        {
            long id = objectIdGenerator.GetId(node, out _);

            switch (node)
            {
                case IfStatement ifStatement:
                    {
                        var conditionId = objectIdGenerator.GetId(ifStatement.Condition, out _);
                        var thenId = objectIdGenerator.GetId(ifStatement.ThenBody, out _);
                        var elseId = objectIdGenerator.GetId(ifStatement.ElseBody, out _);

                        edges.Add($"  {id} -> {conditionId} [label=\"if\"];");
                        edges.Add($"  {id} -> {thenId} [label=\"then\"];");
                        edges.Add($"  {id} -> {elseId} [label=\"else\"];");
                    }

                    break;
                case WhileStatement whileStatement:
                    {
                        var conditionId = objectIdGenerator.GetId(whileStatement.Condition, out _);
                        var bodyId = objectIdGenerator.GetId(whileStatement.Body, out _);

                        edges.Add($"  {id} -> {conditionId};");
                        edges.Add($"  {id} -> {bodyId} [label=\"do\"];");
                    }

                    break;
                case VariableDeclaration variableDeclaration:
                    foreach (var child in node.Children())
                    {
                        var childId = objectIdGenerator.GetId(child, out _);
                        edges.Add($"  {id} -> {childId} [label=\"=\"];");
                    }

                    break;
                case Variable variable:
                    {
                        var declarationId = objectIdGenerator.GetId(variable.Declaration, out _);
                        edges.Add($"  {id} -> {declarationId} [style=dashed, constraint=false];");

                        foreach (var child in node.Children())
                        {
                            long childId = objectIdGenerator.GetId(child, out _);
                            edges.Add($"  {id} -> {childId};");
                        }
                    }

                    break;
                case FunctionDeclaration functionDeclaration:
                    {
                        foreach (var param in functionDeclaration.Parameters)
                        {
                            long paramId = objectIdGenerator.GetId(param, out _);
                            edges.Add($"  {id} -> {paramId};");
                        }

                        var bodyId = objectIdGenerator.GetId(functionDeclaration.Body, out _);
                        edges.Add($"  {id} -> {bodyId} [label=body];");
                    }

                    break;
                case FunctionCall functionCall:
                    {
                        var declarationId = objectIdGenerator.GetId(functionCall.Declaration, out _);
                        edges.Add($"  {id} -> {declarationId} [style=dashed, constraint=false];");

                        foreach (var child in node.Children())
                        {
                            long childId = objectIdGenerator.GetId(child, out _);
                            edges.Add($"  {id} -> {childId};");
                        }
                    }

                    break;
                default:
                    foreach (var child in node.Children())
                    {
                        long childId = objectIdGenerator.GetId(child, out _);
                        edges.Add($"  {id} -> {childId};");
                    }

                    break;
            }

            foreach (var child in node.Children())
            {
                ProcessNodeEdges(child, edges, objectIdGenerator);
            }
        }
    }
}

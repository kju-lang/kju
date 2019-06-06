namespace KJU.Core.AST.TypeChecker
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AST.Types;

    public class SolutionNormalizer
    {
        public const string SolutionContainsUninstantiatedTypeVariable = "SolutionNormalizer.UninstantiatedTypeVariable";
        public const string SolutionContainsArrayRecursion = "SolutionNormalizer.ArrayRecursion";

        /// Normalize the solution - make sure that there are no TypeVariables on right hand sides.
        /// Throw if there is invalid recursion (e.g. X == [X]). Notably, recursion in function types
        /// is allowed (e.g. X -> Y == X).
        /// Throw if solution contains an uninstantiated type variable.
        public static Solution Normalize(Solution solution)
        {
            var mapping = solution.TypeVariableMapping;

            if (ContainsUninstantiatedTypeVariables(mapping))
                throw new SolutionNormalizerException(SolutionContainsUninstantiatedTypeVariable);

            if (ContainsArrayRecursion(mapping))
                throw new SolutionNormalizerException(SolutionContainsArrayRecursion);

            var normalizedMapping = new Dictionary<TypeVariable, IHerbrandObject>();

            // Create placeholders for normalized types
            foreach (var typeVariable in mapping.Keys)
            {
                var value = mapping[typeVariable];
                switch (value)
                {
                    case ArrayType arrayType:
                        // Placeholder ArrayType
                        normalizedMapping[typeVariable] = new ArrayType(new PlaceholderType());
                        break;

                    case FunType funType:
                        // Placeholder FunType
                        normalizedMapping[typeVariable] = new FunType(new List<DataType>(), new PlaceholderType());
                        break;

                    default:
                        // No need for a placeholder as value cannot contain TypeVariables
                        normalizedMapping[typeVariable] = value;
                        break;
                }
            }

            // Tie references
            foreach (var typeVariable in normalizedMapping.Keys)
            {
                var value = mapping[typeVariable];

                switch (normalizedMapping[typeVariable])
                {
                    case ArrayType arrayType:
                        arrayType.ElementType = (DataType)TieReferences(((ArrayType)value).ElementType, normalizedMapping);
                        break;

                    case FunType funType:
                        funType.ResultType = (DataType)TieReferences(((FunType)value).ResultType, normalizedMapping);
                        funType.ArgTypes = ((FunType)value).ArgTypes
                            .Select(type => (DataType)TieReferences(type, normalizedMapping))
                            .ToList();

                        break;
                }
            }

            var normalizedSolution = new Solution()
                { TypeVariableMapping = normalizedMapping, ChosenAlternative = solution.ChosenAlternative };

            return normalizedSolution;
        }

        private static IHerbrandObject TieReferences(IHerbrandObject obj, IDictionary<TypeVariable, IHerbrandObject> mapping)
        {
            switch (obj)
            {
                case TypeVariable typeVariable:
                    return mapping[typeVariable];

                case ArrayType arrayType:
                    return new ArrayType((DataType)TieReferences(arrayType.ElementType, mapping));

                case FunType funType:
                    var argTypes = funType.ArgTypes.Select(type => (DataType)TieReferences(type, mapping));
                    var resultType = (DataType)TieReferences(funType.ResultType, mapping);

                    return new FunType(argTypes, resultType);

                default:
                    return obj;
            }
        }

        private static bool ContainsUninstantiatedTypeVariables(IDictionary<TypeVariable, IHerbrandObject> mapping)
        {
            return mapping.Values
                .SelectMany(GetAllTypeVariables)
                .Any(var => !mapping.ContainsKey(var));
        }

        private static IEnumerable<IHerbrandObject> GetAllHerbrandObjects(IHerbrandObject root)
        {
            yield return root;

            foreach (var obj in root.GetArguments().SelectMany(GetAllHerbrandObjects))
                yield return obj;
        }

        private static IEnumerable<TypeVariable> GetAllTypeVariables(IHerbrandObject root)
        {
            return GetAllHerbrandObjects(root)
                .Where(obj => obj is TypeVariable)
                .Select(obj => (TypeVariable)obj);
        }

        private static bool ContainsArrayRecursion(IDictionary<TypeVariable, IHerbrandObject> mapping)
        {
            var graph = mapping.ToDictionary(
                entry => (IHerbrandObject)entry.Key,
                entry => (ICollection<IHerbrandObject>)new List<IHerbrandObject>() { entry.Value });

            mapping.Values
                .SelectMany(GetAllHerbrandObjects)
                .Where(obj => !(obj is TypeVariable))
                .Distinct()
                .ToList()
                .ForEach(obj => graph[obj] = (ICollection<IHerbrandObject>)obj.GetArguments().ToList());

            // Check if there is an ArrayType that lies on a directed cycle
            return GetStronglyConnectedComponents(graph)
                .Where(component => component.Count > 1)
                .SelectMany(component => component)
                .Any(obj => obj is ArrayType);
        }

        private static ICollection<ICollection<T>> GetStronglyConnectedComponents<T>(IDictionary<T, ICollection<T>> graph)
        {
            var reversedGraph = ReverseGraph(graph);

            var visited = new HashSet<T>();
            var reversedPostOrder = new List<T>();

            graph.Keys.ToList().ForEach(vertex => Dfs(vertex, reversedGraph, visited, reversedPostOrder));
            reversedPostOrder.Reverse();

            var stronglyConnectedComponents = new List<ICollection<T>>();
            visited.Clear();

            foreach (var vertex in reversedPostOrder)
            {
                if (!visited.Contains(vertex))
                {
                    var component = new List<T>();
                    Dfs(vertex, graph, visited, component);

                    stronglyConnectedComponents.Add(component);
                }
            }

            return stronglyConnectedComponents;
        }

        private static IDictionary<T, ICollection<T>> ReverseGraph<T>(IDictionary<T, ICollection<T>> graph)
        {
            Dictionary<T, ICollection<T>> reversedGraph = new Dictionary<T, ICollection<T>>();

            foreach (var vertex in graph.Keys)
                reversedGraph[vertex] = new List<T>();

            foreach (var vertex in graph.Keys)
                graph[vertex].ToList().ForEach(neighbor => reversedGraph[neighbor].Add(vertex));

            return reversedGraph;
        }

        private static void Dfs<T>(T start, IDictionary<T, ICollection<T>> graph, ISet<T> visited, ICollection<T> result)
        {
            if (visited.Contains(start))
                return;

            visited.Add(start);
            graph[start].ToList().ForEach(neighbor => Dfs(neighbor, graph, visited, result));

            // Vertices are added to the result in the order of leaving them
            result.Add(start);
        }

        private class PlaceholderType : DataType
        {
            public override bool IsHeapType()
            {
                throw new NotImplementedException();
            }
        }
    }
}

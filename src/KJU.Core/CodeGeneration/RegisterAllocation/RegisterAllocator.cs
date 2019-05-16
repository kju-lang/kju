namespace KJU.Core.CodeGeneration.RegisterAllocation
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Coalescing;
    using Coloring;
    using Intermediate;
    using LivenessAnalysis;
    using Sorter;
    using Graph =
        System.Collections.Generic.Dictionary<System.Collections.Generic.HashSet<Intermediate.VirtualRegister>,
            System.Collections.Generic.HashSet<System.Collections.Generic.HashSet<Intermediate.VirtualRegister>>>;

    public class RegisterAllocator : IRegisterAllocator
    {
        public RegisterAllocationResult Allocate(
            InterferenceCopyGraphPair query,
            IReadOnlyCollection<HardwareRegister> allowedHardwareRegisters)
        {
            var allowedRegistersCount = allowedHardwareRegisters.Count;

            query = new InterferenceCopyGraphPair(
                query.InterferenceGraph,
                RemoveUnavailableHWRegisters(query.CopyGraph, allowedHardwareRegisters));

            var allRegisters = query.GetAllRegisters();
            var superVertices = allRegisters
                .ToDictionary(
                    x => x,
                    x => new HashSet<VirtualRegister> { x });

            var interference = RegisterAllocatorUtils.GetGraph(allRegisters, superVertices, query.InterferenceGraph);

            interference.AddHardwareRegisterClique(superVertices);

            var copy = RegisterAllocatorUtils.GetGraph(allRegisters, superVertices, query.CopyGraph);

            var coalescingProcess = new CoalescingProcess(interference, copy, superVertices, allowedRegistersCount);
            var registerSorter =
                new RegisterSorter(
                    interference,
                    copy,
                    superVertices,
                    allRegisters,
                    coalescingProcess,
                    allowedHardwareRegisters.Count);

            var order = registerSorter.GetOrder().ToList();
            var verticesEnumerable = allRegisters.Select(register => superVertices[register]);
            var vertices = new HashSet<HashSet<VirtualRegister>>(verticesEnumerable);
            var finalInterference = FinalGraph(allRegisters, superVertices, query.InterferenceGraph);
            var finalCopy = FinalGraph(allRegisters, superVertices, query.CopyGraph);
            var registerPainter = new RegisterPainter(finalInterference, finalCopy, superVertices);
            return registerPainter.GetColoring(vertices, allowedHardwareRegisters, order, allRegisters);
        }

        private static Graph FinalGraph(
            IEnumerable<VirtualRegister> allRegisters,
            IReadOnlyDictionary<VirtualRegister, HashSet<VirtualRegister>> superVertices,
            IReadOnlyDictionary<VirtualRegister, IReadOnlyCollection<VirtualRegister>> graph)
        {
            var finalGraph = new Dictionary<HashSet<VirtualRegister>, HashSet<HashSet<VirtualRegister>>>();
            foreach (var register in allRegisters)
            {
                if (!finalGraph.ContainsKey(superVertices[register]))
                {
                    finalGraph.Add(superVertices[register], new HashSet<HashSet<VirtualRegister>>());
                }
            }

            foreach (var register in graph.Keys)
            {
                foreach (var neighbour in graph[register])
                {
                    if (superVertices[register] != superVertices[neighbour])
                    {
                        finalGraph[superVertices[register]].Add(superVertices[neighbour]);
                        finalGraph[superVertices[neighbour]].Add(superVertices[register]);
                    }
                }
            }

            return finalGraph;
        }

        private static IReadOnlyDictionary<VirtualRegister, IReadOnlyCollection<VirtualRegister>> RemoveUnavailableHWRegisters(
                IReadOnlyDictionary<VirtualRegister, IReadOnlyCollection<VirtualRegister>> graph,
                IReadOnlyCollection<HardwareRegister> allowedHardwareRegisters)
        {
            Func<VirtualRegister, bool> filter =
                register => !(register is HardwareRegister) || allowedHardwareRegisters.Contains(register);

            return graph
                .Where(vertex => filter(vertex.Key))
                .ToDictionary(
                    vertex => vertex.Key,
                    vertex => new HashSet<VirtualRegister>(
                        vertex.Value.Where(filter)) as IReadOnlyCollection<VirtualRegister>);
        }
    }
}
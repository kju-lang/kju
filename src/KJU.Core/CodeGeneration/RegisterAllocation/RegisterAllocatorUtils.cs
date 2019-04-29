namespace KJU.Core.CodeGeneration.RegisterAllocation
{
    using System.Collections.Generic;
    using System.Linq;
    using Intermediate;
    using LivenessAnalysis;
    using Graph =
        System.Collections.Generic.Dictionary<System.Collections.Generic.HashSet<Intermediate.VirtualRegister>,
            System.Collections.Generic.HashSet<System.Collections.Generic.HashSet<Intermediate.VirtualRegister>>>;

    internal static class RegisterAllocatorUtils
    {
        internal static void AddHardwareRegisterClique(
            this IReadOnlyDictionary<HashSet<VirtualRegister>, HashSet<HashSet<VirtualRegister>>> interference,
            IReadOnlyDictionary<VirtualRegister, HashSet<VirtualRegister>> superVertices)
        {
            HardwareRegister.Values.SelectMany(current =>
                    HardwareRegister.Values
                        .Select(other => new { First = current, Second = other }))
                .Where(x => x.First != x.Second)
                .Select(x => new { First = superVertices[x.First], Second = superVertices[x.Second] })
                .ToList()
                .ForEach(x => interference[x.First].Add(x.Second));
        }

        internal static List<VirtualRegister> GetAllRegisters(this InterferenceCopyGraphPair query)
        {
            return new HashSet<VirtualRegister>(
                    query.InterferenceGraph.Keys
                        .Concat(query.CopyGraph.Keys)
                        .Concat(HardwareRegister.Values))
                .ToList();
        }

        internal static Graph GetGraph(
            IEnumerable<VirtualRegister> allRegisters,
            IReadOnlyDictionary<VirtualRegister, HashSet<VirtualRegister>> superVertices,
            IReadOnlyDictionary<VirtualRegister, IReadOnlyCollection<VirtualRegister>> graph)
        {
            return allRegisters.ToDictionary(
                register => superVertices[register],
                register => graph
                    .TryGetValue(register, out var neighbourhood)
                    ? new HashSet<HashSet<VirtualRegister>>(neighbourhood
                        .Select(neighbour => superVertices[neighbour]).ToList())
                    : new HashSet<HashSet<VirtualRegister>>());
        }
    }
}
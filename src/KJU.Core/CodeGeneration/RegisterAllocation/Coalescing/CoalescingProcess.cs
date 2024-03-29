#pragma warning disable SA1008 // Opening parenthesis must not be preceded by a space.
namespace KJU.Core.CodeGeneration.RegisterAllocation.Coalescing
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Intermediate;
    using Predicates;
    using Graph =
        System.Collections.Generic.Dictionary<System.Collections.Generic.HashSet<Intermediate.VirtualRegister>,
            System.Collections.Generic.HashSet<System.Collections.Generic.HashSet<Intermediate.VirtualRegister>>>;

    internal class CoalescingProcess : ICoalescingProcess
    {
        private readonly Graph interference;
        private readonly Graph copy;
        private readonly Dictionary<VirtualRegister, HashSet<VirtualRegister>> superVertices;
        private readonly List<ICoalescePredicate> coalescePredicates;
        private readonly int allowedRegistersCount;

        public CoalescingProcess(
            Graph interference,
            Graph copy,
            Dictionary<VirtualRegister, HashSet<VirtualRegister>> superVertices,
            int allowedRegistersCount)
        {
            this.interference = interference;
            this.copy = copy;
            this.superVertices = superVertices;
            var briggsPredicate = new BriggsPredicate(this.interference, this.copy, allowedRegistersCount);
            var georgePredicate = new GeorgePredicate(this.interference, this.copy, allowedRegistersCount);
            this.coalescePredicates = new List<ICoalescePredicate> { briggsPredicate, georgePredicate };
            this.allowedRegistersCount = allowedRegistersCount;
        }

        public HashSet<HashSet<VirtualRegister>> ContainHardware { get; set; }

        public bool CoalesceOne()
        {
            var pair = this.FindPair();
            if (pair == null)
            {
                return false;
            }

            this.Coalesce(pair);
            return true;
        }

        private void Coalesce(Tuple<HashSet<VirtualRegister>, HashSet<VirtualRegister>> pair)
        {
            var (u, v) = pair;

            if (u.Count < v.Count)
            {
                (u, v) = (v, u);
            }

            u.UnionWith(v);
            if (this.ContainHardware.Contains(v))
            {
                this.ContainHardware.Add(u);
            }

            foreach (var register in u)
            {
                this.superVertices[register] = u;
            }

            this.interference[u].UnionWith(this.interference[v]);
            this.interference.Remove(v);

            foreach (var vertex in this.interference[u])
            {
                this.interference[vertex].Remove(v);
                this.interference[vertex].Add(u);
            }

            this.copy[u].UnionWith(this.copy[v]);
            this.copy.Remove(v);

            this.copy[u].Remove(u);
            this.copy[u].Remove(v);

            foreach (var vertex in this.copy[u])
            {
                this.copy[vertex].Remove(v);
                this.copy[vertex].Add(u);
            }
        }

        private Tuple<HashSet<VirtualRegister>, HashSet<VirtualRegister>> FindPair()
        {
            foreach (var pair in this.copy.SelectMany(copyEntry =>
                    copyEntry.Value.Select(neighbour =>
                    {
                        var vertex = copyEntry.Key;
                        return new Tuple<HashSet<VirtualRegister>, HashSet<VirtualRegister>>(vertex, neighbour);
                    })))
            {
                if (this.coalescePredicates[1].CanCoalesce(pair.Item1, pair.Item2))
                    return pair;
            }

            List<Tuple<int, HashSet<VirtualRegister>>> vertices = new List<Tuple<int, HashSet<VirtualRegister>>>();

            foreach (var vertex in this.copy.Keys)
            {
                int bigDegree = 0;

                foreach (var x in this.interference[vertex])
                {
                    var degree = this.interference[vertex].Count;
                    if (degree >= this.allowedRegistersCount + 1)
                        ++bigDegree;
                }

                vertices.Add(new Tuple<int, HashSet<VirtualRegister>>( bigDegree, vertex ));
            }

            vertices.Sort((x, y) => x.Item1.CompareTo(y.Item1) );

            for (int i = 0; i < vertices.Count; i++)
            {
                for (int j = 0; j < vertices.Count; j++)
                {
                    if (this.coalescePredicates[0].CanCoalesce(vertices[i].Item2, vertices[j].Item2))
                        return new Tuple<HashSet<VirtualRegister>, HashSet<VirtualRegister>>(vertices[i].Item2, vertices[j].Item2);
                }
            }

            return null;
        }
    }
}
namespace KJU.Core.CodeGeneration.RegisterAllocation.Coalescing.Predicates
{
    using System.Collections.Generic;
    using System.Linq;
    using Intermediate;
    using Graph =
        System.Collections.Generic.Dictionary<System.Collections.Generic.HashSet<Intermediate.VirtualRegister>,
            System.Collections.Generic.HashSet<System.Collections.Generic.HashSet<Intermediate.VirtualRegister>>>;

    internal class BriggsPredicate : ICoalescePredicate
    {
        private readonly Graph interference;
        private readonly Graph copy;
        private readonly int allowedRegistersCount;

        public BriggsPredicate(
            Graph interference,
            Graph copy,
            int allowedRegistersCount)
        {
            this.interference = interference;
            this.copy = copy;
            this.allowedRegistersCount = allowedRegistersCount;
        }

        public bool CanCoalesce(HashSet<VirtualRegister> u, HashSet<VirtualRegister> v)
        {
            if (this.interference[u].Contains(v))
            {
                return false;
            }

            if (!this.copy[u].Contains(v))
            {
                return false;
            }

            var neighbours = new HashSet<HashSet<VirtualRegister>>(this.interference[u].Concat(this.interference[v]));

            var bigDegreeCount = neighbours.Count(vertex =>
            {
                var previousDegree = this.interference[vertex].Count;
                var resultDegree = this.interference[vertex].Contains(u) && this.interference[vertex].Contains(v)
                    ? previousDegree - 1
                    : previousDegree;

                return resultDegree >= this.allowedRegistersCount;
            });

            return bigDegreeCount < this.allowedRegistersCount;
        }
    }
}
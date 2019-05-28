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

            int bigDegree = 0;

            foreach (var vertex in this.interference[u])
            {
                var degree = this.interference[vertex].Count;
                if (this.interference[v].Contains(vertex))
                    degree--;
                if (degree >= this.allowedRegistersCount)
                    ++bigDegree;
            }

            foreach (var vertex in this.interference[v])
            {
                if (this.interference[u].Contains(vertex))
                    continue;

                var degree = this.interference[vertex].Count;
                if (degree >= this.allowedRegistersCount)
                    ++bigDegree;
            }

            return bigDegree < this.allowedRegistersCount;
        }
    }
}
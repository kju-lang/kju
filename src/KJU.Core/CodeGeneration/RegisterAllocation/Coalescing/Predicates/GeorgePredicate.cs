namespace KJU.Core.CodeGeneration.RegisterAllocation.Coalescing.Predicates
{
    using System.Collections.Generic;
    using System.Linq;
    using Intermediate;
    using Graph =
        System.Collections.Generic.Dictionary<System.Collections.Generic.HashSet<Intermediate.VirtualRegister>,
            System.Collections.Generic.HashSet<System.Collections.Generic.HashSet<Intermediate.VirtualRegister>>>;

    internal class GeorgePredicate : ICoalescePredicate
    {
        private readonly Graph interference;
        private readonly Graph copy;
        private readonly int allowedRegistersCount;

        public GeorgePredicate(
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

            return !this.interference[u].Any(neighbour =>
                this.interference[neighbour].Count >= this.allowedRegistersCount &&
                !this.interference[neighbour].Contains(v));
        }
    }
}
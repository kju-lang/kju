namespace KJU.Core.CodeGeneration.RegisterAllocation.Sorter
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Coalescing;
    using Intermediate;
    using Graph =
        System.Collections.Generic.Dictionary<System.Collections.Generic.HashSet<Intermediate.VirtualRegister>,
            System.Collections.Generic.HashSet<System.Collections.Generic.HashSet<Intermediate.VirtualRegister>>>;

    internal class RegisterSorter
    {
        private readonly Graph interference;
        private readonly Graph copy;
        private readonly Dictionary<VirtualRegister, HashSet<VirtualRegister>> superVertices;
        private readonly IReadOnlyCollection<VirtualRegister> allRegisters;
        private readonly HashSet<HashSet<VirtualRegister>> containHardware;
        private readonly ICoalescingProcess coalescingProcess;
        private readonly int allowedHardwareRegistersCount;
        private readonly Stack<HashSet<VirtualRegister>> resultStack;

        public RegisterSorter(
            Graph interference,
            Graph copy,
            Dictionary<VirtualRegister, HashSet<VirtualRegister>> superVertices,
            IReadOnlyCollection<VirtualRegister> allRegisters,
            ICoalescingProcess coalescingProcess,
            int allowedHardwareRegistersCount)
        {
            this.interference = interference;
            this.copy = copy;
            this.superVertices = superVertices;
            this.coalescingProcess = coalescingProcess;
            this.allowedHardwareRegistersCount = allowedHardwareRegistersCount;
            this.allRegisters = allRegisters;
            this.containHardware =
                new HashSet<HashSet<VirtualRegister>>(allRegisters
                    .OfType<HardwareRegister>()
                    .Select(x => superVertices[x]));
            this.resultStack = new Stack<HashSet<VirtualRegister>>();
        }

        public IEnumerable<HashSet<VirtualRegister>> GetOrder()
        {
            while (true)
            {
                var allHardware = this.GetVertices().All(vertex => this.containHardware.Contains(vertex));

                if (allHardware)
                {
                    break;
                }

                if (this.RemoveSmallVertexWhere(
                    this.GetVertices(),
                    vertex => this.copy[vertex].Count == 0))
                {
                    continue;
                }

                if (this.coalescingProcess.CoalesceOne())
                {
                    continue;
                }

                if (this.RemoveSmallVertexWhere(this.GetVertices(), vertex => true))
                {
                    continue;
                }

                this.RemoveBiggestVertex(this.GetVertices());
            }

            return this.resultStack.ToList();
        }

        private IEnumerable<HashSet<VirtualRegister>> GetVertices()
        {
            return this.allRegisters
                .Select(register => this.superVertices[register])
                .Where(superVertex => this.interference.ContainsKey(superVertex));
        }

        private bool RemoveSmallVertexWhere(
            IEnumerable<HashSet<VirtualRegister>> vertices,
            Func<HashSet<VirtualRegister>, bool> predicate)
        {
            var lowDegreeVertex = this.GetSmallRegisters(vertices)
                .FirstOrDefault(predicate);

            if (lowDegreeVertex != null)
            {
                this.AcceptVertex(lowDegreeVertex);
                return true;
            }

            return false;
        }

        private IEnumerable<HashSet<VirtualRegister>> GetSmallRegisters(IEnumerable<HashSet<VirtualRegister>> vertices)
        {
            return vertices
                .Where(vertex => !this.containHardware.Contains(vertex))
                .Where(vertex => this.interference[vertex].Count < this.allowedHardwareRegistersCount);
        }

        private void RemoveBiggestVertex(
            IEnumerable<HashSet<VirtualRegister>> vertices)
        {
            var biggestVertex = this.FindBiggestVertex(vertices);
            this.AcceptVertex(biggestVertex);
        }

        private HashSet<VirtualRegister> FindBiggestVertex(IEnumerable<HashSet<VirtualRegister>> vertices)
        {
            var virtualRegisters = vertices.Where(x => !this.containHardware.Contains(x)).ToList();
            var bigDegreeVertex = virtualRegisters
                .Skip(1)
                .Aggregate(
                    virtualRegisters.First(),
                    (result, current) =>
                        this.interference[current].Count > this.interference[result].Count ? current : result);
            return bigDegreeVertex;
        }

        private void AcceptVertex(HashSet<VirtualRegister> vertex)
        {
            this.resultStack.Push(vertex);
            foreach (var neighbour in this.interference[vertex])
            {
                this.interference[neighbour].Remove(vertex);
            }

            foreach (var neighbour in this.copy[vertex])
            {
                this.copy[neighbour].Remove(vertex);
            }

            this.interference.Remove(vertex);
            this.copy.Remove(vertex);
        }
    }
}
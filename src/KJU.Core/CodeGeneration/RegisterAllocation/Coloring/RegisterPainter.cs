namespace KJU.Core.CodeGeneration.RegisterAllocation.Coloring
{
    using System.Collections.Generic;
    using System.Linq;
    using Intermediate;
    using Graph =
        System.Collections.Generic.Dictionary<System.Collections.Generic.HashSet<Intermediate.VirtualRegister>,
            System.Collections.Generic.HashSet<System.Collections.Generic.HashSet<Intermediate.VirtualRegister>>>;

    internal class RegisterPainter
    {
        private readonly Graph interference;
        private readonly Graph copy;
        private readonly Dictionary<VirtualRegister, HashSet<VirtualRegister>> superVertices;

        public RegisterPainter(
            Graph interference,
            Graph copy,
            Dictionary<VirtualRegister, HashSet<VirtualRegister>> superVertices)
        {
            this.interference = interference;
            this.copy = copy;
            this.superVertices = superVertices;
        }

        public RegisterAllocationResult GetColoring(
            IEnumerable<HashSet<VirtualRegister>> vertices,
            IReadOnlyCollection<HardwareRegister> allowedHardwareRegisters,
            IEnumerable<HashSet<VirtualRegister>> order,
            IEnumerable<VirtualRegister> allRegisters)
        {
            var hardwareRegistersColoring =
                allRegisters.OfType<HardwareRegister>()
                    .ToDictionary(
                        register => this.superVertices[register],
                        register => register);

            var vertexColoring = order.Aggregate(hardwareRegistersColoring, (currentColoring, vertex) =>
                {
                    var color = this.GetVertexColor(vertex, currentColoring, allowedHardwareRegisters);
                    currentColoring.Add(vertex, color);
                    return currentColoring;
                })
                .Where(x => x.Value != null)
                .ToDictionary(x => x.Key, x => x.Value);

            var registerColoring = vertexColoring
                .SelectMany(x =>
                    x.Key
                        .Select(register => new KeyValuePair<VirtualRegister, HardwareRegister>(register, x.Value)))
                .ToDictionary(x => x.Key, x => x.Value);

            var spilledVertices = vertices
                .Where(x => !vertexColoring.ContainsKey(x))
                .ToList();

            var spilledRegisters = spilledVertices.SelectMany(superVertex => superVertex).ToList();
            return new RegisterAllocationResult(registerColoring, spilledRegisters);
        }

        private static HardwareRegister FirstFitColor(
            IEnumerable<HardwareRegister> colors,
            IReadOnlyCollection<HardwareRegister> forbidden)
        {
            return colors.FirstOrDefault(color => !forbidden.Contains(color));
        }

        private static HardwareRegister ColorOfCopyNeighbour(
            IReadOnlyDictionary<HashSet<VirtualRegister>, HardwareRegister> coloring,
            IReadOnlyCollection<HardwareRegister> forbidden,
            IEnumerable<HashSet<VirtualRegister>> copyNeighbourhood)
        {
            return copyNeighbourhood
                .Select(copyVertex => coloring.TryGetValue(copyVertex, out var result) ? result : null)
                .FirstOrDefault(color => color != null && !forbidden.Contains(color));
        }

        private HashSet<HardwareRegister> GetForbiddenColors(
            IReadOnlyDictionary<HashSet<VirtualRegister>, HardwareRegister> coloring,
            HashSet<VirtualRegister> copyVertex)
        {
            return new HashSet<HardwareRegister>(this.interference[copyVertex]
                .Select(neighbour => coloring.TryGetValue(neighbour, out var color) ? color : null)
                .Where(color => color != null));
        }

        private HardwareRegister ColorAllowingForAtLeastOneCopy(
            IReadOnlyDictionary<HashSet<VirtualRegister>, HardwareRegister> coloring,
            IReadOnlyCollection<HardwareRegister> colors,
            IReadOnlyCollection<HardwareRegister> forbidden,
            IEnumerable<HashSet<VirtualRegister>> copyNeighbourhood)
        {
            return copyNeighbourhood
                .Where(copyVertex => !coloring.ContainsKey(copyVertex))
                .Select(copyVertex => this.GetForbiddenColors(coloring, copyVertex))
                .SelectMany(omit => colors.Where(x => !omit.Contains(x)))
                .Where(color => color != null)
                .FirstOrDefault(color => !forbidden.Contains(color));
        }

        private HardwareRegister GetVertexColor(
            HashSet<VirtualRegister> vertex,
            IReadOnlyDictionary<HashSet<VirtualRegister>, HardwareRegister> coloring,
            IReadOnlyCollection<HardwareRegister> colors)
        {
            var forbidden = this.GetForbiddenColors(coloring, vertex);

            return ColorOfCopyNeighbour(coloring, forbidden, this.copy[vertex]) ??
                   this.ColorAllowingForAtLeastOneCopy(coloring, colors, forbidden, this.copy[vertex]) ??
                   FirstFitColor(colors, forbidden);
        }
    }
}
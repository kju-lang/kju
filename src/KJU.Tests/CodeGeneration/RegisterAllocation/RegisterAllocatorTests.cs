namespace KJU.Tests.CodeGeneration.RegisterAllocation
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using KJU.Core.CodeGeneration.LivenessAnalysis;
    using KJU.Core.CodeGeneration.RegisterAllocation;
    using KJU.Core.Intermediate;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class RegisterAllocatorTests
    {
        private readonly IRegisterAllocator registerAllocator = new RegisterAllocator();

        [TestMethod]
        public void EmptyGraph()
        {
            var interference = new Dictionary<VirtualRegister, IReadOnlyCollection<VirtualRegister>>();
            var copies = new Dictionary<VirtualRegister, IReadOnlyCollection<VirtualRegister>>();
            var result = this.SimpleTest(interference, copies, HardwareRegister.Values);
            Assert.AreEqual(0, result.Spilled.Count);
        }

        [TestMethod]
        public void OneMoreThanColorable()
        {
            var interference = this.GetClique(17);
            var copies = new Dictionary<VirtualRegister, IReadOnlyCollection<VirtualRegister>>();
            var result = this.SimpleTest(interference, copies, HardwareRegister.Values);
            Assert.AreEqual(1, result.Spilled.Count);
        }

        [TestMethod]
        public void PreferBetterColoringOverCopyEdge()
        {
            /*
             Path on 4 vertices with copy edge between ends.
                Possible only 2 colors.
             */

            var v1 = new NamedVirtualRegister("V1");
            var v2 = new NamedVirtualRegister("V2");
            var v3 = new NamedVirtualRegister("V3");
            var v4 = new NamedVirtualRegister("V4");
            var interference = new Dictionary<VirtualRegister, IReadOnlyCollection<VirtualRegister>>
            {
                [v1] = new List<VirtualRegister> { v2 },
                [v2] = new List<VirtualRegister> { v1, v3 },
                [v3] = new List<VirtualRegister> { v2, v4 },
                [v4] = new List<VirtualRegister> { v3 },
            };
            var copies = new Dictionary<VirtualRegister, IReadOnlyCollection<VirtualRegister>>
            {
                [v1] = new List<VirtualRegister> { v4 },
                [v4] = new List<VirtualRegister> { v1 },
            };
            var registers = new List<HardwareRegister>
            {
                HardwareRegister.RAX,
                HardwareRegister.RBX
            };
            var result = this.SimpleTest(interference, copies, registers);
            Assert.AreEqual(0, result.Spilled.Count);
        }

        [TestMethod]
        public void UseCopyEdgeWhenPossible()
        {
            /*
             Graph: 1 <-> 2, 3 <-> 4
                1 wants to have same color as 4.
                Possible 3 colors but 2 is enough.
             */

            var v1 = new NamedVirtualRegister("V1");
            var v2 = new NamedVirtualRegister("V2");
            var v3 = new NamedVirtualRegister("V3");
            var v4 = new NamedVirtualRegister("V4");
            var interference = new Dictionary<VirtualRegister, IReadOnlyCollection<VirtualRegister>>
            {
                [v1] = new List<VirtualRegister> { v2 },
                [v2] = new List<VirtualRegister> { v1 },
                [v3] = new List<VirtualRegister> { v4 },
                [v4] = new List<VirtualRegister> { v3 },
            };
            var copies = new Dictionary<VirtualRegister, IReadOnlyCollection<VirtualRegister>>
            {
                [v1] = new List<VirtualRegister> { v4 },
                [v4] = new List<VirtualRegister> { v1 },
            };
            var registers = new List<HardwareRegister>
            {
                HardwareRegister.RAX,
                HardwareRegister.RBX,
                HardwareRegister.RCX
            };
            var result = this.SimpleTest(interference, copies, registers);
            Assert.AreEqual(0, result.Spilled.Count);
            Assert.AreEqual(result.Allocation[v1], result.Allocation[v4]);
        }

        [TestMethod]
        public void RandomTreeTest()
        {
            /*
             * Tree - always 2-colorable
             */
            var random = new Random();

            var virtualRegisters = Enumerable.Range(1, 1000).Select(index => new NamedVirtualRegister($"V{index}"))
                .ToList();
            var interference = new Dictionary<VirtualRegister, IReadOnlyCollection<VirtualRegister>>
            {
                [virtualRegisters[0]] = new HashSet<VirtualRegister>()
            };

            for (var i = 1; i < virtualRegisters.Count; i++)
            {
                var register = virtualRegisters[i];
                var parentIndex = random.Next(i);
                var parent = virtualRegisters[parentIndex];
                interference.Add(register, new HashSet<VirtualRegister> { parent });
                ((HashSet<VirtualRegister>)interference[parent]).Add(register);
            }

            var copies = new Dictionary<VirtualRegister, IReadOnlyCollection<VirtualRegister>>();
            var registers = new List<HardwareRegister>
            {
                HardwareRegister.RAX,
                HardwareRegister.RBX,
            };
            var result = this.SimpleTest(interference, copies, registers);
            Assert.AreEqual(0, result.Spilled.Count);
        }

        [DataRow(10, 1)]
        [DataRow(100, 2)]
        [DataRow(1000, 3)]
        [DataRow(3000, 4)]
        [DataTestMethod]
        public void RandomTreeModifiedTest(int vertexCount, int seed)
        {
            /*
             * Tree with leafs capped by 2 super vertices. Best case spills max 2.
             */
            var random = new Random(seed);
            var virtualRegisters = Enumerable.Range(1, vertexCount)
                .Select(index => new NamedVirtualRegister($"V{index}"))
                .ToList();
            var interference = new Dictionary<VirtualRegister, IReadOnlyCollection<VirtualRegister>>
            {
                [virtualRegisters[0]] = new HashSet<VirtualRegister>()
            };
            var superVertices = new List<VirtualRegister>
            {
                new NamedVirtualRegister("Super0"),
                new NamedVirtualRegister("Super1"),
            };
            for (var i = 1; i < virtualRegisters.Count; i++)
            {
                var register = virtualRegisters[i];
                var parentIndex = random.Next(i);
                var parent = virtualRegisters[parentIndex];
                interference.Add(register, new HashSet<VirtualRegister> { parent });
                ((HashSet<VirtualRegister>)interference[parent]).Add(register);
            }

            var superInterference = new HashSet<VirtualRegister>();
            foreach (var register in virtualRegisters)
            {
                var currentInterference = (HashSet<VirtualRegister>)interference[register];
                if (currentInterference.Count == 1)
                {
                    currentInterference.UnionWith(superVertices);
                    superInterference.Add(register);
                }
            }

            var interference0 = new HashSet<VirtualRegister>(superInterference.Append(superVertices[1]));
            var interference1 = new HashSet<VirtualRegister>(superInterference.Append(superVertices[0]));
            interference.Add(superVertices[0], interference0);
            interference.Add(superVertices[1], interference1);

            var copies = new Dictionary<VirtualRegister, IReadOnlyCollection<VirtualRegister>>();
            var registers = new List<HardwareRegister>
            {
                HardwareRegister.RAX,
                HardwareRegister.RBX,
            };
            var result = this.SimpleTest(interference, copies, registers);
            Console.WriteLine(
                $"Test size: {vertexCount}. Best case spill: max 2. Actual spill {result.Spilled.Count}");
        }

        private static List<string> CheckResult(
            InterferenceCopyGraphPair query,
            IEnumerable<HardwareRegister> allowedHardwareRegisters,
            RegisterAllocationResult result)
        {
            var interference = query.InterferenceGraph;
            var registerMapping = result.Allocation;
            var spilled = result.Spilled;

            var notAllowedHardwareRegisterUse = HardwareRegister.Values
                .Where(register => !allowedHardwareRegisters.Contains(register))
                .Select(register => new
                {
                    Register = register,
                    Mapping = registerMapping
                        .Where(x => x.Key != register && x.Value == register)
                        .Select(x => x.Key)
                        .ToList()
                })
                .Where(x => x.Mapping.Any())
                .Select(failedRegister =>
                {
                    var mappingsText = string.Join(", ", failedRegister.Mapping);
                    return
                        $"Hardware register '{failedRegister.Register}' is not allowed but used for {{{mappingsText}}}.";
                });

            var notMappedHardwareRegisters = HardwareRegister.Values
                .Where(register => !registerMapping.ContainsKey(register))
                .Select(register => $"Hardware register '{register}' not mapped.");

            var incorrectlyMappedHardwareRegisters = HardwareRegister.Values
                .Where(register => registerMapping.ContainsKey(register))
                .Select(register => new
                {
                    Register = registerMapping[register],
                    Mapping = register
                })
                .Where(register => register.Register != register.Mapping)
                .Select(register =>
                    $"Hardware register '{register.Register}' not mapped to itself but to {register.Mapping}.");

            var notMappedRegistersFromInterference = interference.Keys
                .Where(register => !(registerMapping.ContainsKey(register) || spilled.Contains(register)))
                .Select(register => $"Register '{register}' from interference not mapped or spilled.");

            var bothMappedAndSpilled = spilled
                .Where(registerMapping.ContainsKey)
                .Select(register => $"Register '{register}' both mapped and spilled.");

            var conflictingRegisters = interference
                .Where(register => registerMapping.ContainsKey(register.Key))
                .Select(mapping => new
                {
                    Register = mapping.Key,
                    Conflicts = mapping.Value
                        .Where(y => registerMapping.TryGetValue(y, out var yMapping) &&
                                    registerMapping[mapping.Key] == yMapping)
                        .ToList()
                })
                .Where(x => x.Conflicts.Any())
                .SelectMany(x => x.Conflicts
                    .Select(conflictingRegister =>
                        $"Register '{x.Register}' interferes with {conflictingRegister} and both got register {registerMapping[conflictingRegister]}"));

            return notAllowedHardwareRegisterUse
                .Concat(notMappedHardwareRegisters)
                .Concat(incorrectlyMappedHardwareRegisters)
                .Concat(notMappedRegistersFromInterference)
                .Concat(bothMappedAndSpilled)
                .Concat(conflictingRegisters)
                .ToList();
        }

        private RegisterAllocationResult SimpleTest(
            IReadOnlyDictionary<VirtualRegister, IReadOnlyCollection<VirtualRegister>> interference,
            IReadOnlyDictionary<VirtualRegister, IReadOnlyCollection<VirtualRegister>> copies,
            IReadOnlyCollection<HardwareRegister> allowedHardwareRegisters)
        {
            var query = new InterferenceCopyGraphPair(interference, copies);
            var result = this.registerAllocator.Allocate(query, allowedHardwareRegisters);
            var errors = CheckResult(query, allowedHardwareRegisters, result);
            if (errors.Any())
            {
                var message = string.Join(Environment.NewLine, errors);
                Assert.Fail(message);
            }

            return result;
        }

        private IReadOnlyDictionary<VirtualRegister, IReadOnlyCollection<VirtualRegister>> GetClique(
            int size, string namePrefix = "clique")
        {
            var registers = Enumerable.Range(1, size)
                .Select(x => new NamedVirtualRegister($"{namePrefix}_{x}"))
                .Cast<VirtualRegister>()
                .ToList();

            return registers.Select(register => new
            {
                Key = register,
                Value = registers.Where(other => other != register).ToList()
            }).ToDictionary(x => x.Key, x => (IReadOnlyCollection<VirtualRegister>)x.Value);
        }

        private class NamedVirtualRegister : VirtualRegister
        {
            private readonly string name;

            public NamedVirtualRegister(string name)
            {
                this.name = name;
            }

            public override string ToString()
            {
                return this.name;
            }
        }
    }
}
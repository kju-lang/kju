#pragma warning disable SA1008  // Opening parenthesis must not be preceded by a space.
namespace KJU.Core.CodeGeneration.FunctionToAsmGeneration
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AST;
    using Intermediate;
    using LivenessAnalysis;
    using RegisterAllocation;
    using Templates;

    public static class FunctionToAsmGenerator
    {
        private const int AllocationTriesBound = 5;

        public static IEnumerable<string> ToAsm(FunctionDeclaration functionDeclaration)
        {
            var iFunction = functionDeclaration.IntermediateFunction;
            var cfg = FunctionCfg(functionDeclaration);
            var instructionSequence = InstructionSequence(cfg);

            var templates = InstructionsTemplatesFactory.CreateInstructionTemplates();
            var instructionSelector = new InstructionSelector(templates);

            var livenessAnalyzer = new LivenessAnalyzer();
            var registerAllocator = new RegisterAllocator();

            RegisterAllocationResult allocation = null;

            for (var iteration = 0; iteration < AllocationTriesBound; ++iteration)
            {
                var interferenceCopyGraphPair = livenessAnalyzer.GetInterferenceCopyGraphs(instructionSequence);
                var allocationResult = registerAllocator.Allocate(interferenceCopyGraphPair, HardwareRegister.Values);
                var spilled = allocationResult.Spilled.ToList();

                if (spilled.Count == 0)
                {
                    allocation = allocationResult;
                    break;
                }

                var spilledRegisterToIndexMapping = new Dictionary<VirtualRegister, int>();
                for (var i = 0; i < spilled.Count; ++i)
                {
                    spilledRegisterToIndexMapping.Add(spilled[i], i);
                }

                var newInstructionSequence = new List<Tuple<Label, IReadOnlyList<Instruction>>>();

                foreach (var (label, block) in instructionSequence)
                {
                    var modifiedBlock = new List<Instruction>();
                    foreach (var instruction in block)
                    {
                        var auxiliaryReads = instruction.Uses
                            .Where(register => spilled.Contains(register))
                            .SelectMany(register =>
                            {
                                var id = spilledRegisterToIndexMapping[register];
                                var registerVariable = new Intermediate.Variable(iFunction, register);
                                var memoryLocation = new MemoryLocation(iFunction, iFunction.StackBytes + (8 * id));
                                var memoryVariable = new Intermediate.Variable(iFunction, memoryLocation);
                                var readOperation = iFunction.GenerateRead(registerVariable);
                                var writeOperation = iFunction.GenerateWrite(memoryVariable, readOperation);
                                return instructionSelector.Select(new Tree(writeOperation));
                            });

                        var auxiliaryWrites = instruction.Defines
                            .Where(register => spilled.Contains(register))
                            .SelectMany(register =>
                            {
                                var id = spilledRegisterToIndexMapping[register];
                                var registerVariable = new Intermediate.Variable(iFunction, register);
                                var memoryLocation = new MemoryLocation(iFunction, iFunction.StackBytes + (8 * id));
                                var memoryVariable = new Intermediate.Variable(iFunction, memoryLocation);
                                var readOperation = iFunction.GenerateRead(memoryVariable);
                                var writeOperation = iFunction.GenerateWrite(registerVariable, readOperation);
                                return instructionSelector.Select(new Tree(writeOperation));
                            });

                        modifiedBlock.AddRange(auxiliaryReads);
                        modifiedBlock.Add(instruction);
                        modifiedBlock.AddRange(auxiliaryWrites);
                    }

                    var newLabelBlockPair = Tuple.Create(label, (IReadOnlyList<Instruction>)modifiedBlock);
                    newInstructionSequence.Add(newLabelBlockPair);
                }

                instructionSequence = newInstructionSequence;
                iFunction.StackBytes += 8 * spilled.Count;
            }

            if (allocation == null)
            {
                throw new FunctionToAsmGeneratorException(
                    $"Cannot allocate registers without spills after {AllocationTriesBound} times");
            }

            yield return $"{iFunction.MangledName}:{Environment.NewLine}";
            foreach (var (label, block) in instructionSequence)
            {
                yield return $"{label.Id}:{Environment.NewLine}";
                foreach (var instruction in block)
                {
                    yield return instruction.ToASM(allocation.Allocation);
                }
            }
        }

        private static Label FunctionCfg(FunctionDeclaration functionDeclaration)
        {
            var bodyGenerator = new FunctionBodyGenerator(functionDeclaration.IntermediateFunction);
            return bodyGenerator.BuildFunctionBody(functionDeclaration.Body);
        }

        private static IReadOnlyList<Tuple<Label, IReadOnlyList<Instruction>>> InstructionSequence (Label cfg)
        {
            var (orderedTrees, labelToIndexMapping) = new CFGLinearizer().Linearize(cfg);

            var indexToLabelsMapping = new HashSet<Label>[labelToIndexMapping.Count];
            labelToIndexMapping.ToList().ForEach(kvp =>
            {
                var (label, index) = (kvp.Key, kvp.Value);
                if (indexToLabelsMapping[index] == null)
                {
                    indexToLabelsMapping[index] = new HashSet<Label>();
                }

                indexToLabelsMapping[index].Add(label);
            });

            var nopInstruction = new NopInstruction();
            var nopInstructionBlock = new List<Instruction> { nopInstruction } as IReadOnlyList<Instruction>;

            var templates = InstructionsTemplatesFactory.CreateInstructionTemplates();
            var instructionSelector = new InstructionSelector(templates);

            return orderedTrees.SelectMany((tree, index) =>
            {
                var labelsWithNops = indexToLabelsMapping[index]
                    .Select(label => Tuple.Create(label, nopInstructionBlock));

                var auxiliaryLabel = new Label(tree);
                var block = instructionSelector.Select(tree).ToList() as IReadOnlyList<Instruction>;

                var labelBlockTuple = Tuple.Create(auxiliaryLabel, block);

                return labelsWithNops.Append(labelBlockTuple);
            }).ToList();
        }
    }
}

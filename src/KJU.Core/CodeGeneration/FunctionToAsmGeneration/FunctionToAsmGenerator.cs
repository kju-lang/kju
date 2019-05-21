#pragma warning disable SA1008  // Opening parenthesis must not be preceded by a space.
namespace KJU.Core.CodeGeneration.FunctionToAsmGeneration
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using CfgLinearizer;
    using InstructionSelector;
    using Intermediate;
    using Intermediate.Function;
    using Intermediate.FunctionGeneration.ReadWrite;
    using LivenessAnalysis;
    using RegisterAllocation;
    using Templates;

    public class FunctionToAsmGenerator : IFunctionToAsmGenerator
    {
        private const int AllocationTriesBound = 4;

        private readonly ILivenessAnalyzer livenessAnalyzer;
        private readonly IRegisterAllocator registerAllocator;
        private readonly IInstructionSelector instructionSelector;
        private readonly ICfgLinearizer cfgLinearizer;
        private readonly ILabelFactory labelFactory;
        private readonly ReadWriteGenerator readWriteGenerator;

        public FunctionToAsmGenerator(
            ILivenessAnalyzer livenessAnalyzer,
            IRegisterAllocator registerAllocator,
            IInstructionSelector instructionSelector,
            ICfgLinearizer cfgLinearizer,
            ILabelFactory labelFactory,
            ReadWriteGenerator readWriteGenerator)
        {
            this.livenessAnalyzer = livenessAnalyzer;
            this.registerAllocator = registerAllocator;
            this.instructionSelector = instructionSelector;
            this.cfgLinearizer = cfgLinearizer;
            this.labelFactory = labelFactory;
            this.readWriteGenerator = readWriteGenerator;
        }

        public IEnumerable<string> ToAsm(Function function, ILabel cfg)
        {
            var (allocation, instructionSequence) = this.Allocate(this.InstructionSequence(cfg), function);
            return ConstructResult(instructionSequence, allocation, function);
        }

        public IEnumerable<string> GenerateLayout(Function function)
        {
            throw new NotImplementedException();
        }

        private static IEnumerable<string> UsefulLabels(IEnumerable<CodeBlock> instructionSequence)
        {
            foreach (var instruction in instructionSequence.SelectMany(block => block.Instructions))
            {
                // No call, since we only look for local labels.
                switch (instruction)
                {
                    case UnconditionalJumpInstruction instr:
                        yield return instr.Label.Id;
                        break;
                    case ConditionalJumpTemplate.ConditionalJumpInstruction instr:
                        yield return instr.Label;
                        break;
                    default:
                        break;
                }
            }
        }

        private static IEnumerable<string> ConstructResult(
            IReadOnlyList<CodeBlock> instructionSequence,
            RegisterAllocationResult allocation,
            Function function)
        {
            var usefulLabels = new HashSet<string>(UsefulLabels(instructionSequence));
            return instructionSequence.SelectMany(codeBlock =>
            {
                var ret = codeBlock.Instructions.SelectMany(instruction => instruction.ToASM(allocation.Allocation));
                return !usefulLabels.Contains(codeBlock.Label.Id) ? ret : ret.Prepend($"{codeBlock.Label.Id}:");
            }).Prepend($"{function.MangledName}:");
        }

        private (RegisterAllocationResult, IReadOnlyList<CodeBlock>) Allocate(
            IReadOnlyList<CodeBlock> instructionSequence,
            Function function)
        {
            for (var iteration = 0; iteration < AllocationTriesBound; ++iteration)
            {
                var interferenceCopyGraphPair = this.livenessAnalyzer.GetInterferenceCopyGraphs(instructionSequence);

                var allowedHardwareRegisters = HardwareRegister.Values;

                var allocationResult =
                    this.registerAllocator.Allocate(interferenceCopyGraphPair, allowedHardwareRegisters);
                var spilled = new HashSet<VirtualRegister>(allocationResult.Spilled);

                if (spilled.Count == 0)
                {
                    return (allocationResult, instructionSequence);
                }

                instructionSequence = this.PushSpilledOnStack(spilled, instructionSequence, function);
            }

            throw new FunctionToAsmGeneratorException(
                $"Cannot allocate registers without spills after {AllocationTriesBound} times");
        }

        private IReadOnlyList<CodeBlock> PushSpilledOnStack(
            ICollection<VirtualRegister> spilled,
            IEnumerable<CodeBlock> instructionSequence,
            Function function)
        {
            var spilledRegisterMemory = spilled
                .ToDictionary(x => x, x => function.ReserveStackFrameLocation());

            var result = instructionSequence.Select(codeBlock =>
            {
                var modifiedInstructions = codeBlock.Instructions.SelectMany(instruction =>
                    this.GetModifiedInstruction(
                        instruction,
                        spilled,
                        spilledRegisterMemory,
                        function)).ToList();

                return new CodeBlock(codeBlock.Label, modifiedInstructions);
            }).ToList();

            return result;
        }

        private IEnumerable<Instruction> GetModifiedInstruction(
            Instruction instruction,
            ICollection<VirtualRegister> spilled,
            IReadOnlyDictionary<VirtualRegister, MemoryLocation> spilledRegisterMemory,
            Function function)
        {
            var auxiliaryWrites = instruction.Defines
                .Where(spilled.Contains)
                .SelectMany(register =>
                {
                    var memoryLocation = spilledRegisterMemory[register];
                    var readOperation = this.readWriteGenerator.GenerateRead(function, register);
                    var writeOperation = this.readWriteGenerator.GenerateWrite(function, memoryLocation, readOperation);
                    var tree = new Tree(writeOperation, new UnconditionalJump(null));
                    return this.instructionSelector.GetInstructions(tree);
                });

            var auxiliaryReads = instruction.Uses
                .Where(spilled.Contains)
                .SelectMany(register =>
                {
                    var memoryLocation = spilledRegisterMemory[register];
                    var readOperation = this.readWriteGenerator.GenerateRead(function, memoryLocation);
                    var writeOperation = this.readWriteGenerator.GenerateWrite(function, register, readOperation);
                    var tree = new Tree(writeOperation, new UnconditionalJump(null));
                    return this.instructionSelector.GetInstructions(tree);
                });

            return auxiliaryReads.Append(instruction).Concat(auxiliaryWrites);
        }

        private IReadOnlyList<CodeBlock> InstructionSequence(ILabel cfg)
        {
            var (orderedTrees, labelToIndexMapping) = this.cfgLinearizer.Linearize(cfg);

            var indexToLabelsMapping =
                labelToIndexMapping
                    .GroupBy(kvp => kvp.Value, kvp => kvp.Key)
                    .ToDictionary(x => x.Key, x => new HashSet<ILabel>(x));

            return orderedTrees.SelectMany((tree, index) =>
            {
                var labelsWithNops = indexToLabelsMapping[index]
                    .Select(label =>
                    {
                        var nopInstruction = new NopInstruction();
                        var nopInstructionBlock =
                            new List<Instruction> { nopInstruction } as IReadOnlyList<Instruction>;
                        label.Tree = new Tree(null, new UnconditionalJump(null));
                        return new CodeBlock(label, nopInstructionBlock);
                    });

                var auxiliaryLabel = this.labelFactory.GetLabel(tree);
                var block = this.instructionSelector.GetInstructions(tree).ToList() as IReadOnlyList<Instruction>;

                var labelBlockTuple = new CodeBlock(auxiliaryLabel, block);

                return labelsWithNops.Append(labelBlockTuple);
            }).ToList();
        }
    }
}
#pragma warning disable SA1008  // Opening parenthesis must not be preceded by a space.
namespace KJU.Core.CodeGeneration.FunctionToAsmGeneration
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AST;
    using InstructionSelector;
    using Intermediate;
    using Intermediate.FunctionBodyGenerator;
    using LivenessAnalysis;
    using RegisterAllocation;

    public class FunctionToAsmGenerator : IFunctionToAsmGenerator
    {
        private const int AllocationTriesBound = 5;

        private readonly ILivenessAnalyzer livenessAnalyzer;
        private readonly IRegisterAllocator registerAllocator;
        private readonly IInstructionSelector instructionSelector;
        private readonly ILabelIdGenerator labelIdGenerator;
        private readonly CFGLinearizer.CFGLinearizer cfgLinearizer;

        public FunctionToAsmGenerator(
            ILivenessAnalyzer livenessAnalyzer,
            IRegisterAllocator registerAllocator,
            IInstructionSelector instructionSelector,
            ILabelIdGenerator labelIdGenerator,
            CFGLinearizer.CFGLinearizer cfgLinearizer)
        {
            this.livenessAnalyzer = livenessAnalyzer;
            this.registerAllocator = registerAllocator;
            this.instructionSelector = instructionSelector;
            this.labelIdGenerator = labelIdGenerator;
            this.cfgLinearizer = cfgLinearizer;
        }

        public IEnumerable<string> ToAsm(FunctionDeclaration functionDeclaration)
        {
            var function = functionDeclaration.IntermediateFunction;
            var cfg = FunctionCfg(functionDeclaration);
            var (allocation, instructionSequence) = this.Allocate(function, this.InstructionSequence(cfg));
            return ConstructResult(instructionSequence, allocation, function);
        }

        private static IEnumerable<string> ConstructResult(
            IEnumerable<CodeBlock> instructionSequence,
            RegisterAllocationResult allocation,
            Function function)
        {
            return instructionSequence.SelectMany(codeBlock =>
            {
                return codeBlock.Instructions.Select(instruction => instruction.ToASM(allocation.Allocation))
                    .Prepend($"{codeBlock.Label.Id}:{Environment.NewLine}");
            }).Prepend($"{function.MangledName}:{Environment.NewLine}");
        }

        private static Label FunctionCfg(FunctionDeclaration functionDeclaration)
        {
            var bodyGenerator = new FunctionBodyGenerator(functionDeclaration.IntermediateFunction);
            return bodyGenerator.BuildFunctionBody(functionDeclaration.Body);
        }

        private (RegisterAllocationResult, IReadOnlyList<CodeBlock>) Allocate(
            Function function, IReadOnlyList<CodeBlock> instructionSequence)
        {
            for (var iteration = 0; iteration < AllocationTriesBound; ++iteration)
            {
                var interferenceCopyGraphPair = this.livenessAnalyzer.GetInterferenceCopyGraphs(instructionSequence);
                var allocationResult =
                    this.registerAllocator.Allocate(interferenceCopyGraphPair, HardwareRegister.Values);
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
            var spilledRegisterToIndexMapping = spilled
                .Select((register, index) => new { Register = register, Index = index })
                .ToDictionary(x => x.Register, x => x.Index);

            var result = instructionSequence.Select(codeBlock =>
            {
                var modifiedInstructions = codeBlock.Instructions.SelectMany(instruction =>
                    this.GetModifiedInstruction(
                        instruction,
                        spilled,
                        spilledRegisterToIndexMapping,
                        function)).ToList();

                return new CodeBlock(codeBlock.Label, modifiedInstructions);
            }).ToList();

            function.StackBytes += 8 * spilled.Count;
            return result;
        }

        private IEnumerable<Instruction> GetModifiedInstruction(
            Instruction instruction,
            ICollection<VirtualRegister> spilled,
            IReadOnlyDictionary<VirtualRegister, int> spilledRegisterToIndexMapping,
            Function function)
        {
            var auxiliaryReads = instruction.Uses
                .Where(spilled.Contains)
                .SelectMany(register =>
                {
                    var id = spilledRegisterToIndexMapping[register];
                    var registerVariable = new Intermediate.Variable(function, register);
                    var memoryLocation = new MemoryLocation(function, function.StackBytes + (8 * id));
                    var memoryVariable = new Intermediate.Variable(function, memoryLocation);
                    var readOperation = function.GenerateRead(registerVariable);
                    var writeOperation = function.GenerateWrite(memoryVariable, readOperation);
                    return this.instructionSelector.GetInstructions(new Tree(writeOperation));
                });

            var auxiliaryWrites = instruction.Defines
                .Where(spilled.Contains)
                .SelectMany(register =>
                {
                    var id = spilledRegisterToIndexMapping[register];
                    var registerVariable = new Intermediate.Variable(function, register);
                    var memoryLocation = new MemoryLocation(function, function.StackBytes + (8 * id));
                    var memoryVariable = new Intermediate.Variable(function, memoryLocation);
                    var readOperation = function.GenerateRead(memoryVariable);
                    var writeOperation = function.GenerateWrite(registerVariable, readOperation);
                    return this.instructionSelector.GetInstructions(new Tree(writeOperation));
                });

            return auxiliaryReads.Append(instruction).Concat(auxiliaryWrites);
        }

        private IReadOnlyList<CodeBlock> InstructionSequence(Label cfg)
        {
            var (orderedTrees, labelToIndexMapping) = this.cfgLinearizer.Linearize(cfg);

            var indexToLabelsMapping =
                labelToIndexMapping
                    .GroupBy(kvp => kvp.Value, kvp => kvp.Key)
                    .ToDictionary(x => x.Key, x => new HashSet<Label>(x));

            var nopInstruction = new NopInstruction();
            var nopInstructionBlock = new List<Instruction> { nopInstruction } as IReadOnlyList<Instruction>;

            return orderedTrees.SelectMany((tree, index) =>
            {
                var labelsWithNops = indexToLabelsMapping[index]
                    .Select(label => new CodeBlock(label, nopInstructionBlock));

                var auxiliaryLabel = new Label(tree);
                var block = this.instructionSelector.GetInstructions(tree).ToList() as IReadOnlyList<Instruction>;

                var labelBlockTuple = new CodeBlock(auxiliaryLabel, block);

                return labelsWithNops.Append(labelBlockTuple);
            }).ToList();
        }
    }
}
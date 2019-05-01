#pragma warning disable SA1118  // Parameter must not span multiple lines

namespace KJU.Tests.CodeGeneration.LivenessAnalysis
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using KJU.Core.CodeGeneration;
    using KJU.Core.CodeGeneration.FunctionToAsmGeneration;
    using KJU.Core.CodeGeneration.LivenessAnalysis;
    using KJU.Core.Intermediate;
    using KJU.Tests.Util;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class LivenessAnalyzerTests
    {
        [TestMethod]
        public void TestEmpty()
        {
            var emptyGraph = this.GetEmptyGraph(new List<VirtualRegister>());

            var instructions = new List<CodeBlock>();

            this.CheckAnswer(instructions, emptyGraph, emptyGraph);

            this.AddRetBlock(instructions, this.GetLabel(), new List<Instruction>());

            this.CheckAnswer(instructions, emptyGraph, emptyGraph);
        }

        [TestMethod]
        public void TestUnused()
        {
            var aReg = new VirtualRegister();
            var bReg = new VirtualRegister();
            var cReg = new VirtualRegister();

            var instructions = new List<CodeBlock>();

            var block = new List<Instruction>() {
                this.GetDefinition(aReg),
                this.GetDefinition(bReg),
                this.GetDefinition(cReg),
                this.GetDefinition(aReg),
                this.GetDefinition(bReg),
                this.GetDefinition(cReg)
            };

            this.AddRetBlock(instructions, this.GetLabel(), block);

            var registers = new List<VirtualRegister>() { aReg, bReg, cReg };
            var emptyGraph = this.GetEmptyGraph(registers);

            this.CheckAnswer(instructions, emptyGraph, emptyGraph);
        }

        [TestMethod]
        public void TestOperation()
        {
            var aReg = new VirtualRegister();
            var bReg = new VirtualRegister();
            var outReg = new VirtualRegister();

            var instructions = new List<CodeBlock>();

            var block = new List<Instruction>() {
                this.GetDefinition(aReg),
                this.GetDefinition(bReg),
                this.GetOperation(aReg, bReg, outReg)
            };

            this.AddRetBlock(instructions, this.GetLabel(), block);

            var registers = new List<VirtualRegister>() { aReg, bReg, outReg };

            var interferenceGraph = this.GetEmptyGraph(registers);
            var copyGraph = this.GetEmptyGraph(registers);

            this.AddEdge(interferenceGraph, aReg, bReg);

            this.AddEdge(copyGraph, aReg, outReg);
            this.AddEdge(copyGraph, bReg, outReg);

            this.CheckAnswer(instructions, interferenceGraph, copyGraph);
        }

        [TestMethod]
        public void TestOverwrite()
        {
            var aReg = new VirtualRegister();
            var bReg = new VirtualRegister();
            var outReg = new VirtualRegister();

            var instructions = new List<CodeBlock>();

            var block = new List<Instruction>
            {
                this.GetDefinition(aReg),
                this.GetDefinition(bReg),
                this.GetOperation(bReg, bReg, outReg),
                this.GetDefinition(aReg),
                this.GetOperation(aReg, aReg, outReg)
            };

            this.AddRetBlock(instructions, this.GetLabel(), block);

            var registers = new List<VirtualRegister> { aReg, bReg, outReg };

            var interferenceGraph = this.GetEmptyGraph(registers);
            var copyGraph = this.GetEmptyGraph(registers);

            this.AddEdge(copyGraph, aReg, outReg);
            this.AddEdge(copyGraph, bReg, outReg);

            this.CheckAnswer(instructions, interferenceGraph, copyGraph);
        }

        [TestMethod]
        public void TestSelfAssignment()
        {
            var reg = new VirtualRegister();

            var instructions = new List<CodeBlock>();

            this.AddRetBlock(instructions, this.GetLabel(), new List<Instruction>() {
                this.GetDefinition(reg),
                this.GetOperation(reg, reg, reg)
            });

            var emptyGraph = this.GetEmptyGraph(new List<VirtualRegister>() { reg });

            this.CheckAnswer(instructions, emptyGraph, emptyGraph);
        }

        [TestMethod]
        public void TestBranch()
        {
            var aReg = new VirtualRegister();
            var bReg = new VirtualRegister();
            var cReg = new VirtualRegister();

            var aLabel = this.GetLabel();
            var bLabel = this.GetLabel();
            var cLabel = this.GetLabel();

            var aBlock = new List<Instruction>() {
                this.GetDefinition(aReg)
            };

            var bBlock = new List<Instruction>() {
                this.GetDefinition(bReg),
                this.GetOperation(aReg, aReg, bReg)
            };

            var cBlock = new List<Instruction>() {
                this.GetDefinition(cReg),
                this.GetOperation(cReg, cReg, aReg)
            };

            var instructions = new List<CodeBlock>();

            this.AddConditionalJumpBlock(instructions, aLabel, aBlock, bLabel, cLabel);
            this.AddRetBlock(instructions, bLabel, bBlock);
            this.AddRetBlock(instructions, cLabel, cBlock);

            var registers = new List<VirtualRegister>() { aReg, bReg, cReg };

            var interferenceGraph = this.GetEmptyGraph(registers);
            var copyGraph = this.GetEmptyGraph(registers);

            this.AddEdge(interferenceGraph, aReg, bReg);

            this.AddEdge(copyGraph, aReg, bReg);
            this.AddEdge(copyGraph, aReg, cReg);

            this.CheckAnswer(instructions, interferenceGraph, copyGraph);
        }

        [TestMethod]
        public void TestFunction()
        {
            var aReg = new VirtualRegister();
            var bReg = new VirtualRegister();
            var outReg = new VirtualRegister();

            var afterLabel = this.GetLabel();

            var beforeBlock = new List<Instruction>() {
                this.GetDefinition(aReg),
                this.GetDefinition(bReg)
            };

            var afterBlock = new List<Instruction>() {
                this.GetOperation(aReg, bReg, outReg)
            };

            var instructions = new List<CodeBlock>();

            this.AddFunctionCallBlock(instructions, this.GetLabel(), beforeBlock, afterLabel);
            this.AddRetBlock(instructions, afterLabel, afterBlock);

            var registers = new List<VirtualRegister>() { aReg, bReg, outReg };

            var interferenceGraph = this.GetEmptyGraph(registers);
            var copyGraph = this.GetEmptyGraph(registers);

            this.AddEdge(interferenceGraph, aReg, bReg);

            this.AddEdge(copyGraph, aReg, outReg);
            this.AddEdge(copyGraph, bReg, outReg);

            this.CheckAnswer(instructions, interferenceGraph, copyGraph);
        }

        [TestMethod]
        public void TestLoop()
        {
            var aReg = new VirtualRegister();
            var bReg = new VirtualRegister();
            var outReg = new VirtualRegister();

            var label = this.GetLabel();

            var block = new List<Instruction>() {
                this.GetOperation(aReg, aReg, outReg),
                this.GetDefinition(aReg),
                this.GetDefinition(bReg),
                this.GetOperation(bReg, bReg, outReg)
            };

            var registers = new List<VirtualRegister>() { aReg, bReg, outReg };
            var copyGraph = this.GetEmptyGraph(registers);

            this.AddEdge(copyGraph, aReg, outReg);
            this.AddEdge(copyGraph, bReg, outReg);

            /* Behavior when there is no loop */

            var instructionsWithoutLoop = new List<CodeBlock>();
            this.AddRetBlock(instructionsWithoutLoop, label, block);

            var interferenceGraphWithoutLoop = this.GetEmptyGraph(registers);

            this.CheckAnswer(instructionsWithoutLoop, interferenceGraphWithoutLoop, copyGraph);

            /* Different behavior when there is a loop */

            var instructionsWithLoop = new List<CodeBlock>();
            this.AddUnconditionalJumpBlock(instructionsWithLoop, label, block, label);

            var interferenceGraphWithLoop = this.GetEmptyGraph(registers);

            this.AddEdge(interferenceGraphWithLoop, aReg, bReg);
            this.AddEdge(interferenceGraphWithLoop, aReg, outReg);

            this.CheckAnswer(instructionsWithLoop, interferenceGraphWithLoop, copyGraph);
        }

        [TestMethod]
        public void TestDiamond()
        {
            var aReg = new VirtualRegister();
            var bReg = new VirtualRegister();
            var cReg = new VirtualRegister();
            var dReg = new VirtualRegister();

            var leftLabel = this.GetLabel();
            var rightLabel = this.GetLabel();
            var tailLabel = this.GetLabel();

            var headBlock = new List<Instruction>() {
                this.GetDefinition(aReg)
            };

            var leftBlock = new List<Instruction>() {
                this.GetOperation(aReg, aReg, bReg),
                this.GetOperation(bReg, bReg, aReg),
                this.GetOperation(aReg, aReg, bReg),
                this.GetOperation(bReg, bReg, aReg)
            };

            var rightBlock = new List<Instruction>() {
                this.GetDefinition(cReg),
                this.GetDefinition(cReg)
            };

            var tailBlock = new List<Instruction>() {
                this.GetOperation(aReg, dReg, cReg),
                this.GetDefinition(cReg)
            };

            var instructions = new List<CodeBlock>();

            this.AddConditionalJumpBlock(instructions, this.GetLabel(), headBlock, leftLabel, rightLabel);
            this.AddUnconditionalJumpBlock(instructions, leftLabel, leftBlock, tailLabel);
            this.AddUnconditionalJumpBlock(instructions, rightLabel, rightBlock, tailLabel);
            this.AddRetBlock(instructions, tailLabel, tailBlock);

            var registers = new List<VirtualRegister>() { aReg, bReg, cReg, dReg };

            var interferenceGraph = this.GetEmptyGraph(registers);
            var copyGraph = this.GetEmptyGraph(registers);

            this.AddEdge(interferenceGraph, aReg, cReg);
            this.AddEdge(interferenceGraph, aReg, dReg);
            this.AddEdge(interferenceGraph, bReg, dReg);
            this.AddEdge(interferenceGraph, cReg, dReg);

            this.AddEdge(copyGraph, aReg, bReg);
            this.AddEdge(copyGraph, aReg, cReg);
            this.AddEdge(copyGraph, dReg, cReg);

            this.CheckAnswer(instructions, interferenceGraph, copyGraph);
        }

        private Instruction GetDefinition(VirtualRegister register)
        {
            return new InstructionMock(
                new List<VirtualRegister>(),
                new List<VirtualRegister> { register },
                new List<Tuple<VirtualRegister, VirtualRegister>>());
        }

        private Instruction GetOperation(VirtualRegister lhs, VirtualRegister rhs, VirtualRegister result)
        {
            return new InstructionMock(
                new List<VirtualRegister> { lhs, rhs },
                new List<VirtualRegister> { result },
                new List<Tuple<VirtualRegister, VirtualRegister>> {
                    new Tuple<VirtualRegister, VirtualRegister>(lhs, result),
                    new Tuple<VirtualRegister, VirtualRegister>(rhs, result) });
        }

        private void AddRetBlock(
            List<CodeBlock> instructions, Label label, List<Instruction> block)
        {
            block.Add(new RetInstructionMock());
            label.Tree.ControlFlow = new Ret();

            instructions.Add(new CodeBlock(label, block));
        }

        private void AddConditionalJumpBlock(
            List<CodeBlock> instructions, Label label, List<Instruction> block, Label firstTarget, Label secondTarget)
        {
            label.Tree.ControlFlow = new ConditionalJump(firstTarget, secondTarget);
            instructions.Add(new CodeBlock(label, block));
        }

        private void AddUnconditionalJumpBlock(
            List<CodeBlock> instructions, Label label, List<Instruction> block, Label target)
        {
            label.Tree.ControlFlow = new UnconditionalJump(target);
            instructions.Add(new CodeBlock(label, block));
        }

        private void AddFunctionCallBlock(
            List<CodeBlock> instructions, Label label, List<Instruction> block, Label target)
        {
            label.Tree.ControlFlow = new FunctionCall(new Function(), target);
            instructions.Add(new CodeBlock(label, block));
        }

        private void CheckAnswer(
            IReadOnlyList<CodeBlock> instructions,
            Dictionary<VirtualRegister, List<VirtualRegister>> expectedInterferenceGraph,
            Dictionary<VirtualRegister, List<VirtualRegister>> expectedCopyGraph)
        {
            var result = new LivenessAnalyzer().GetInterferenceCopyGraphs(instructions);

            Assert.IsTrue(MappingEquivalence.AreEquivalentCollection(
                expectedInterferenceGraph.ToDictionary(t => t.Key, t => (IReadOnlyCollection<VirtualRegister>)t.Value), result.InterferenceGraph));

            Assert.IsTrue(MappingEquivalence.AreEquivalentCollection(
                expectedCopyGraph.ToDictionary(t => t.Key, t => (IReadOnlyCollection<VirtualRegister>)t.Value), result.CopyGraph));
        }

        private void AddEdge(
            Dictionary<VirtualRegister, List<VirtualRegister>> graph, VirtualRegister register1, VirtualRegister register2)
        {
            graph[register1].Add(register2);
            graph[register2].Add(register1);
        }

        private Label GetLabel()
        {
            return new Label(new Tree(null));
        }

        private Dictionary<VirtualRegister, List<VirtualRegister>> GetEmptyGraph(IReadOnlyCollection<VirtualRegister> registers)
        {
            return registers.ToDictionary(register => register, register => new List<VirtualRegister>());
        }

        internal class InstructionMock : Instruction
        {
            public InstructionMock(
                IReadOnlyCollection<VirtualRegister> uses,
                IReadOnlyCollection<VirtualRegister> defines,
                IReadOnlyCollection<Tuple<VirtualRegister, VirtualRegister>> copies)
                : base(uses, defines, copies)
            {
            }

            public override string ToASM(IReadOnlyDictionary<VirtualRegister, HardwareRegister> registerAssignment)
            {
                throw new NotImplementedException();
            }
        }

        internal class RetInstructionMock : InstructionMock
        {
            public RetInstructionMock()
                : base(null, null, null)
            {
            }
        }
    }
}
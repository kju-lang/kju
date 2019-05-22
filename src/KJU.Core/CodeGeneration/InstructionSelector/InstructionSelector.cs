namespace KJU.Core.CodeGeneration.InstructionSelector
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Intermediate;
    using Templates;

    public class InstructionSelector : IInstructionSelector
    {
        private readonly Dictionary<JumpType, Dictionary<ShapeKey, List<InstructionTemplate>>> templates;

        public InstructionSelector(IEnumerable<InstructionTemplate> templates)
        {
            var templatesDictionary = templates.GroupBy(
                    template =>
                        template.IsConditionalJump ? JumpType.Conditional : JumpType.NonConditional)
                .ToDictionary(
                    jumpGroup => jumpGroup.Key,
                    jumpGroup => jumpGroup);

            this.templates = templatesDictionary.ToDictionary(
                x => x.Key,
                x => x.Value.GroupBy(template => new ShapeKey(template.Shape?.GetType()))
                    .ToDictionary(
                        templateGroup => templateGroup.Key,
                        templateGroup => templateGroup
                            .OrderByDescending(template => template.Score)
                            .ToList()));

            foreach (JumpType jumpType in Enum.GetValues(typeof(JumpType)))
            {
                if (!this.templates.ContainsKey(jumpType))
                {
                    this.templates[jumpType] = new Dictionary<ShapeKey, List<InstructionTemplate>>();
                }
            }
        }

        private enum JumpType
        {
            Conditional,
            NonConditional
        }

        public IEnumerable<Instruction> GetInstructions(Tree tree)
        {
            var node = tree.Root;
            var result = new VirtualRegister();
            JumpType jumpType;
            string label;
            var controlFlow = tree.ControlFlow;
            switch (controlFlow)
            {
                case ConditionalJump jump:
                {
                    jumpType = JumpType.Conditional;
                    label = jump.TrueTarget.Id;
                    if (label == null)
                    {
                        throw new InstructionSelectorException("Label is null.");
                    }

                    break;
                }

                default:
                {
                    jumpType = JumpType.NonConditional;
                    label = null;
                    break;
                }
            }

            var instructions = this.FitTemplates(
                node,
                result,
                this.templates[jumpType],
                label);

            switch (controlFlow)
            {
                case ConditionalJump jump:
                {
                    return jump.FalseTarget == null
                        ? instructions
                        : instructions.Append(new UnconditionalJumpInstruction(jump.FalseTarget));
                }

                case FunctionCall call:
                {
                    if (call.TargetAfter != null)
                    {
                        throw new NotImplementedException("non-trivial calls not supported yet");
                    }

                    var functionInfo = call.Function;
                    return instructions.Append(new CallInstruction(functionInfo));
                }

                case UnconditionalJump jmp:
                {
                    return jmp.Target == null
                        ? instructions
                        : instructions.Append(new UnconditionalJumpInstruction(jmp.Target));
                }

                case Ret _:
                {
                    return instructions.Append(new RetInstruction());
                }

                default:
                    throw new InstructionSelectorException($"Unknown control flow: {controlFlow}");
            }
        }

        private List<object> Fit(Node template, Node root)
        {
            if (template == null)
            {
                return new List<object> { root };
            }

            var rootPlaceholders = root.Match(template);

            if (rootPlaceholders == null)
            {
                return null;
            }

            var childrenFits = root.Children()
                .Zip(
                    template.Children(),
                    (rootNode, templateNode) => this.Fit(templateNode, rootNode))
                .ToList();
            if (childrenFits.Any(x => x == null))
            {
                return null;
            }

            var childrenPlaceholders = childrenFits.SelectMany(x => x);
            return rootPlaceholders.Concat(childrenPlaceholders).ToList();
        }

        private IEnumerable<Instruction> FitTemplates(
            Node node,
            VirtualRegister result,
            IReadOnlyDictionary<ShapeKey, List<InstructionTemplate>> templateDict,
            string label)
        {
            var possibleTemplates = templateDict.TryGetValue(new ShapeKey(node.GetType()), out var value)
                ? value
                : new List<InstructionTemplate>();

            var nullTemplates = templateDict.TryGetValue(new ShapeKey(null), out var extracted)
                ? extracted
                : new List<InstructionTemplate>();

            possibleTemplates.AddRange(nullTemplates);

            var bestMatch = possibleTemplates
                .Select(
                    currentTemplate => new
                        { Template = currentTemplate, Fits = this.Fit(currentTemplate.Shape, node) })
                .FirstOrDefault(x => x.Fits != null);

            if (bestMatch == null)
            {
                throw new InstructionSelectorException($"No matching template for {node}.");
            }

            var fill = new List<object>();
            var childInstructions = Enumerable.Empty<Instruction>();
            foreach (var child in bestMatch.Fits)
            {
                object currentFill;
                if (child is Node innerNode)
                {
                    var register = new VirtualRegister();
                    var innerTemplates = this.FitTemplates(
                        innerNode,
                        register,
                        this.templates[JumpType.NonConditional],
                        null);
                    childInstructions = childInstructions.Concat(innerTemplates);
                    currentFill = register;
                }
                else
                {
                    currentFill = child;
                }

                fill.Add(currentFill);
            }

            return childInstructions.Append(bestMatch.Template.Emit(result, fill, label));
        }

        private class ShapeKey
        {
            private readonly Type shapeType;

            public ShapeKey(Type shapeType)
            {
                this.shapeType = shapeType;
            }

            public override bool Equals(object obj)
            {
                if (obj is ShapeKey other)
                {
                    return this.shapeType == other.shapeType;
                }

                return false;
            }

            public override int GetHashCode()
            {
                return this.shapeType != null ? this.shapeType.GetHashCode() : 0;
            }

            public override string ToString()
            {
                return this.shapeType.ToString();
            }
        }
    }
}
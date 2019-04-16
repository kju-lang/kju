namespace KJU.Core.CodeGeneration
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Intermediate;
    using Templates;

    public class InstructionSelector
    {
        private readonly Dictionary<Type, List<InstructionTemplate>> nonConditionalJumpDict;
        private readonly Dictionary<Type, List<InstructionTemplate>> conditionalJumpDict;

        public InstructionSelector(IEnumerable<InstructionTemplate> templates)
        {
            var dictionaries = templates
                .GroupBy(template => template.IsConditionalJump)
                .ToDictionary(
                    jumpGroup => jumpGroup.Key,
                    jumpGroup => jumpGroup
                        .GroupBy(template => template.Shape.GetType())
                        .ToDictionary(
                            templateGroup => templateGroup.Key,
                            templateGroup => templateGroup
                                .OrderByDescending(template => template.Score)
                                .ToList()));

            this.nonConditionalJumpDict = dictionaries.TryGetValue(false, out var nonJumpDict)
                ? nonJumpDict
                : new Dictionary<Type, List<InstructionTemplate>>();
            this.conditionalJumpDict = dictionaries.TryGetValue(true, out var jumpDict)
                ? jumpDict
                : new Dictionary<Type, List<InstructionTemplate>>();
        }

        public IEnumerable<Instruction> Select(Tree tree)
        {
            var node = tree.Root;
            var result = new VirtualRegister();

            var controlFlow = tree.ControlFlow;
            switch (controlFlow)
            {
                case ConditionalJump jump:
                {
                    var instructions = this.FitTemplates(node, result, this.conditionalJumpDict, jump.TrueTarget.Id);
                    if (jump.FalseTarget != null)
                    {
                        instructions.Add(new UnconditionalJumpInstruction(jump.FalseTarget));
                    }

                    return instructions;
                }

                case FunctionCall call:
                {
                    var instructions = this.FitTemplates(node, result, this.nonConditionalJumpDict, null);

                    if (call.TargetAfter != null)
                    {
                        instructions.Add(new CallInstruction(call.Func));
                    }

                    return instructions;
                }

                case UnconditionalJump jmp:
                {
                    var instructions = this.FitTemplates(node, result, this.nonConditionalJumpDict, null);
                    if (jmp.Target != null)
                    {
                        instructions.Add(new UnconditionalJumpInstruction(jmp.Target));
                    }

                    return instructions;
                }

                case Ret _:
                {
                    return this.FitTemplates(node, result, this.nonConditionalJumpDict, null)
                        .Append(new RetInstruction());
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
                    (rootNode, templateNode) => new { RootNode = rootNode, TemplateNode = templateNode })
                .Select(child => this.Fit(child.TemplateNode, child.RootNode)).ToList();

            if (childrenFits.Any(x => x == null))
            {
                return null;
            }

            var childrenPlaceholders = childrenFits.SelectMany(x => x);
            return rootPlaceholders.Concat(childrenPlaceholders).ToList();
        }

        private IList<Instruction> FitTemplates(
            Node node,
            VirtualRegister result,
            IReadOnlyDictionary<Type, List<InstructionTemplate>> templateDict,
            string label)
        {
            if (!templateDict.TryGetValue(node.GetType(), out var currentCandidates))
            {
                throw new InstructionSelectorException($"No templates for: {node.GetType()}.");
            }

            var bestMatch = currentCandidates
                .Select(currentTemplate => new
                    { Template = currentTemplate, Fits = this.Fit(currentTemplate.Shape, node) })
                .FirstOrDefault(x => x.Fits != null);
            if (bestMatch == null)
            {
                throw new InstructionSelectorException($"No matching template for {node}.");
            }

            var fill = new List<object>();
            var instructions = new List<Instruction>();
            foreach (var child in bestMatch.Fits)
            {
                object currentFill;
                if (child is Node n)
                {
                    var register = new VirtualRegister();
                    instructions.AddRange(this.FitTemplates(n, register, this.nonConditionalJumpDict, null));
                    currentFill = result;
                }
                else
                {
                    currentFill = child;
                }

                fill.Add(currentFill);
            }

            instructions.Add(bestMatch.Template.Emit(result, fill, label));
            return instructions;
        }
    }
}
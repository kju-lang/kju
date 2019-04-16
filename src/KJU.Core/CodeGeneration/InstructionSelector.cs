namespace KJU.Core.CodeGeneration
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Intermediate;
    using Templates;

    public class InstructionSelector
    {
        private Dictionary<Type, List<InstructionTemplate>> nonConditionalJumpDict;
        private Dictionary<Type, List<InstructionTemplate>> conditionalJumpDict;

        public InstructionSelector(IReadOnlyCollection<InstructionTemplate> templates)
        {
            var templateDict = new Dictionary<Type, List<InstructionTemplate>>();
            var conditionalJumpDict = new Dictionary<Type, List<InstructionTemplate>>();

            foreach (var template in templates)
            {
                if (!templateDict.Keys.Contains(template.Shape.GetType()))
                {
                    templateDict.Add(template.Shape.GetType(), new List<InstructionTemplate>());
                    conditionalJumpDict.Add(template.Shape.GetType(), new List<InstructionTemplate>());
                }

                if (template.IsConditionalJump)
                {
                    conditionalJumpDict[template.Shape.GetType()].Add(template);
                }
                else
                {
                    templateDict[template.Shape.GetType()].Add(template);
                }
            }

            this.nonConditionalJumpDict = new Dictionary<Type, List<InstructionTemplate>>();
            this.conditionalJumpDict = new Dictionary<Type, List<InstructionTemplate>>();

            foreach (Type type in templateDict.Keys)
            {
                this.nonConditionalJumpDict.Add(type, templateDict[type].OrderByDescending(t => t.Score).ToList());
                this.conditionalJumpDict.Add(type, conditionalJumpDict[type].OrderByDescending(t => t.Score).ToList());
            }
        }

        public IEnumerable<Instruction> Select(Tree tree)
        {
            List<Instruction> instructions = new List<Instruction>();
            Node node = tree.Root;
            VirtualRegister result = new VirtualRegister();

            if (tree.ControlFlow is ConditionalJump jump)
            {
                this.FitTemplates(node, result, this.conditionalJumpDict, jump.TrueTarget.Id, instructions);
                if (jump.FalseTarget != null)
                {
                    instructions.Add(new UnconditionalJumpInstruction { Label = jump.FalseTarget });
                }
            }
            else if (tree.ControlFlow is FunctionCall call)
            {
                this.FitTemplates(node, result, this.nonConditionalJumpDict, null, instructions);

                if (call.TargetAfter != null)
                {
                    instructions.Add(new CallInstruction { Func = call.Func });
                }
            }
            else if (tree.ControlFlow is UnconditionalJump jmp)
            {
                this.FitTemplates(node, result, this.nonConditionalJumpDict, null, instructions);
                if (jmp.Target != null)
                {
                    instructions.Add(new UnconditionalJumpInstruction { Label = jmp.Target });
                }
            }
            else if (tree.ControlFlow is Ret)
            {
                this.FitTemplates(node, result, this.nonConditionalJumpDict, null, instructions);

                instructions.Add(new RetInstruction());
            }

            return instructions;
        }

        private List<object> Fit(Node template, Node root)
        {
            if (template == null)
            {
                return new List<object> { root };
            }

            List<object> placeholders = root.Match(template);
            if (placeholders == null)
            {
                return null;
            }

            for (int i = 0; i < root.Children().Count; ++i)
            {
                List<object> fitChild = this.Fit(template.Children()[i], root.Children()[i]);
                if (fitChild == null)
                {
                    return null;
                }

                placeholders.AddRange(fitChild);
            }

            return placeholders;
        }

        private void FitTemplates(Node node, VirtualRegister result, Dictionary<Type, List<InstructionTemplate>> templateDict, string label, List<Instruction> instructions)
        {
            if (!templateDict.ContainsKey(node.GetType()))
            {
                throw new InstructionSelectorException("No matching template");
            }

            foreach (var template in templateDict[node.GetType()])
            {
                List<object> fits = this.Fit(template.Shape, node);
                if (fits != null)
                {
                    List<object> fill = new List<object>();
                    foreach (object child in fits)
                    {
                        if (child is Node n)
                        {
                            VirtualRegister register = new VirtualRegister();
                            fill.Add(register);
                            this.FitTemplates(n, register, this.nonConditionalJumpDict, null, instructions);
                        }
                        else
                        {
                            fill.Add(child);
                        }
                    }

                    instructions.Add(template.Emit(result, fill, label));
                    return;
                }
            }

            throw new InstructionSelectorException("No matching template");
        }
    }
}
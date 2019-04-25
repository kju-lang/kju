#pragma warning disable CS0169
#pragma warning disable SA1202 // this class violates SRP, so it's exempt from this warning :) (private members must come before public)
namespace KJU.Core.Intermediate
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using AST.VariableAccessGraph;

    public class Function : IFunction
    {
        public Function(Function parent)
        {
            this.Parent = parent;
        }

        public Function()
        {
        }

        public Variable Link { get; set; }

        public List<Variable> Arguments { get; set; }

        public Function Parent { get; set; }

        public string MangledName { get; set; }

        public int StackBytes { get; set; }

        private static List<ILocation> ArgumentRegisters()
        {
            return new List<ILocation>
            {
                HardwareRegister.RDI,
                HardwareRegister.RSI,
                HardwareRegister.RDX,
                HardwareRegister.RCX,
                HardwareRegister.R8,
                HardwareRegister.R9
            };
        }

        private static Label ConcatTrees(IReadOnlyList<Tree> trees)
        {
            var treesReversed = trees.Reverse().ToList();
            var treesDropped = treesReversed.Skip(1);
            treesDropped
                .Zip(treesReversed, (modified, target) => new { Modified = modified, Target = target })
                .ToList()
                .ForEach(x => { x.Modified.ControlFlow = new UnconditionalJump(new Label(x.Target)); });
            return new Label(trees[0]);
        }

        public MemoryLocation ReserveStackFrameLocation()
        {
            return new MemoryLocation(this, -(this.StackBytes += 8));
        }

        // We will use standard x86-64 conventions -> RDI, RSI, RDX, RCX, R8, R9.
        public Label GenerateCall(
            VirtualRegister result,
            List<VirtualRegister> arguments,
            Label onReturn,
            Function caller)
        {
            if (arguments.Count > 5)
            {
                // Temporary solution
                throw new FunctionObjectException("The function cannot have more than 5 arguments!");
            }

            var argumentsVariables = arguments.Select(argument => new
            {
                ArgumentValue = caller.GenerateRead(new Variable(caller, argument)),
                ArgumentCopyVariable = new Variable(caller, new VirtualRegister()),
            }).ToList();

            var writeArgumentsOperations = argumentsVariables.Select(x =>
            {
                var writeOperation = caller.GenerateWrite(x.ArgumentCopyVariable, x.ArgumentValue);
                return new Tree(writeOperation);
            });

            var argumentsValueTrees =
                argumentsVariables.Select(x => caller.GenerateRead(x.ArgumentCopyVariable));

            var arity = this.Arguments.Count;
            var argumentRegisters = ArgumentRegisters();
            var registersAndValues = argumentRegisters.Zip(
                argumentsValueTrees,
                (register, value) => new { Register = register, Value = value });

            var registerWriteOperations = registersAndValues.Select(x =>
            {
                var registerVariable = new Variable(caller, x.Register);
                return new Tree(caller.GenerateWrite(registerVariable, x.Value));
            });

            var descendant = this.GetDescendant(caller);

            var linkVariable = new Variable(caller, argumentRegisters[arity]);
            var readDescendantLinkOperation = caller.GenerateRead(descendant.Link);
            var writeLinkOperation = caller.GenerateWrite(linkVariable, readDescendantLinkOperation);
            var writeLinkOperationControlFlow = new FunctionCall(this, onReturn);
            var writeLinkOperationTree = new Tree(writeLinkOperation) { ControlFlow = writeLinkOperationControlFlow };

            var operations = writeArgumentsOperations
                .Concat(registerWriteOperations)
                .Append(writeLinkOperationTree)
                .ToList();

            return ConcatTrees(operations);
        }

        public Node GenerateRead(Variable v)
        {
            return this.GenerateRead(v, new RegisterRead(HardwareRegister.RBP));
        }

        public Node GenerateWrite(Variable v, Node value)
        {
            return this.GenerateWrite(v, value, new RegisterRead(HardwareRegister.RBP));
        }

        private Node GenerateRead(Variable v, Node framePointer)
        {
            switch (v.Location)
            {
                case VirtualRegister reg:
                    if (v.Owner != this)
                        throw new ArgumentException("read of virtual register outside its function");
                    return new RegisterRead(reg);
                case MemoryLocation location:
                    return new MemoryRead(this.GenerateVariableLocation(location, framePointer));
                default:
                    throw new ArgumentException($"unexpected Location kind {v}");
            }
        }

        private Node GenerateWrite(Variable v, Node value, Node framePointer)
        {
            switch (v.Location)
            {
                case VirtualRegister reg:
                    Debug.Assert(v.Owner == this, "write to virtual register outside its function");
                    return new RegisterWrite(reg, value);
                case MemoryLocation location:
                    return new MemoryWrite(this.GenerateVariableLocation(location, framePointer), value);
                default:
                    throw new ArgumentException($"unexpected Location kind {v}");
            }
        }

        private Node GenerateVariableLocation(MemoryLocation loc, Node framePointer)
        {
            if (loc.Function == this)
            {
                return new ArithmeticBinaryOperation(
                    AST.ArithmeticOperationType.Addition,
                    framePointer,
                    new IntegerImmediateValue(loc.Offset));
            }
            else
            {
                if (this.Parent == null)
                    throw new ArgumentException("variable not found in parents chain");

                Node parentFramePointer = this.GenerateRead(this.Link, framePointer);
                return this.Parent.GenerateVariableLocation(loc, parentFramePointer);
            }
        }

        public Label GeneratePrologue(Label after)
        {
            var rbpRegister = HardwareRegister.RBP;
            var rbpVariable = new Variable(this, rbpRegister);
            var rbpReadOperation = this.GenerateRead(rbpVariable);
            var rbpPush = new Push(rbpReadOperation);
            var pushRbpOperation = new Tree(rbpPush);

            var rspRegister = HardwareRegister.RSP;
            var rspVariable = new Variable(this, rspRegister);
            var rspRead = this.GenerateRead(rspVariable);
            var rbpWrite = this.GenerateWrite(rbpVariable, rspRead);
            var writeRbpOperation = new Tree(rbpWrite);

            var reserveStackMemoryNode = new ReserveStackMemory(this);
            var moveRspOperation = new Tree(reserveStackMemoryNode);

            var allArguments = this.Arguments.Append(this.Link);
            var argumentLocations = ArgumentRegisters();
            var argumentsWithLocations = allArguments
                .Zip(argumentLocations, (argument, location) => new { argument, location });
            var rewriteParametersOperations = argumentsWithLocations.Select(x =>
            {
                var variable = new Variable(this, x.location);
                var readOperation = this.GenerateRead(variable);
                var writeOperation = this.GenerateWrite(x.argument, readOperation);
                return new Tree(writeOperation);
            });

            var operations = new List<Tree> { pushRbpOperation, writeRbpOperation, moveRspOperation }
                .Concat(rewriteParametersOperations)
                .Append(after.Tree)
                .ToList();
            return ConcatTrees(operations);
        }

        public Label GenerateEpilogue(Node retVal)
        {
            var raxRegister = HardwareRegister.RAX;
            var raxVariable = new Variable(this, raxRegister);

            var writeOperation = this.GenerateWrite(raxVariable, retVal);
            var rbpRegister = HardwareRegister.RBP;
            var popRbpOperation = new Pop(rbpRegister);

            var operations = new List<Tree>
            {
                new Tree(writeOperation),
                new Tree(popRbpOperation) { ControlFlow = new Ret() }
            };

            return ConcatTrees(operations);
        }

        public Label GenerateBody(AST.FunctionDeclaration root)
        {
            this.ExtractTemporaryVariables(root);
            var generator = new FunctionBodyGenerator(this);
            return generator.BuildFunctionBody(root.Body);
        }

        private void ExtractTemporaryVariables(AST.FunctionDeclaration root)
        {
            var variableAccessGraphGenerator = new VariableAccessGraphGenerator(new AST.CallGraph.CallGraphGenerator());
            var variableModificationGraph = variableAccessGraphGenerator.BuildVariableModificationsPerAstNode(root);
            var variableAccessGraph = variableAccessGraphGenerator.BuildVariableAccessesPerAstNode(root);
            var extractor = new TemporaryVariablesExtractor(variableModificationGraph, variableAccessGraph);
            var result = extractor.ExtractTemporaryVariables(root.Body);
            var instructions = result.Concat(root.Body.Instructions).ToList();
            root.Body = new AST.InstructionBlock(instructions);
        }

        private Function GetDescendant(Function caller)
        {
            var result = this.Parent != caller ? caller : this;
            while (result.Parent != this.Parent)
            {
                result = result.Parent;
            }

            return result;
        }
    }
}
namespace KJU.Core.Intermediate.FunctionGeneration.BodyGenerator
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using CallGenerator;
    using NameMangler;
    using PrologueEpilogue;
    using ReadWrite;
    using TemporaryVariablesExtractor;

    public class FunctionBodyGenerator
    {
        private readonly ILabelFactory labelFactory;

        private readonly ReadWriteGenerator readWriteGenerator;

        private readonly PrologueEpilogueGenerator prologueEpilogueGenerator;

        private readonly CallGenerator callGenerator;

        public FunctionBodyGenerator(
            ILabelFactory labelFactory,
            ReadWriteGenerator readWriteGenerator,
            PrologueEpilogueGenerator prologueEpilogueGenerator,
            CallGenerator callGenerator)
        {
            this.labelFactory = labelFactory;
            this.readWriteGenerator = readWriteGenerator;
            this.prologueEpilogueGenerator = prologueEpilogueGenerator;
            this.callGenerator = callGenerator;
        }

        public ILabel BuildFunctionBody(Function.Function function, AST.InstructionBlock body)
        {
            return new GenerateProcess(
                    this.labelFactory,
                    this.readWriteGenerator,
                    this.prologueEpilogueGenerator,
                    this.callGenerator,
                    function)
                .BuildFunctionBody(body);
        }

        private class GenerateProcess
        {
            private readonly ILabelFactory labelFactory;

            private readonly ReadWriteGenerator readWriteGenerator;

            private readonly PrologueEpilogueGenerator prologueEpilogueGenerator;

            private readonly CallGenerator callGenerator;

            private readonly Function.Function function;

            private readonly Dictionary<AST.WhileStatement, LoopLabels> loopLabels =
                new Dictionary<AST.WhileStatement, LoopLabels>();

            public GenerateProcess(
                ILabelFactory labelFactory,
                ReadWriteGenerator readWriteGenerator,
                PrologueEpilogueGenerator prologueEpilogueGenerator,
                CallGenerator callGenerator,
                Function.Function function)
            {
                this.labelFactory = labelFactory;
                this.readWriteGenerator = readWriteGenerator;
                this.prologueEpilogueGenerator = prologueEpilogueGenerator;
                this.callGenerator = callGenerator;
                this.function = function;
            }

            public ILabel BuildFunctionBody(AST.InstructionBlock body)
            {
                var retVal = this.function.IsEntryPoint ? (Node)new IntegerImmediateValue(0) : new UnitImmediateValue();

                var guardEpilogue = this.prologueEpilogueGenerator.GenerateEpilogue(this.function, retVal);
                var afterPrologue = this.ConvertNode(body, guardEpilogue);
                return this.prologueEpilogueGenerator.GeneratePrologue(this.function, afterPrologue.Start);
            }

            // Any method that returns `Computation` prepends the created list
            // of `Tree`s (that begins at `Start`) to `after`. It's up to the caller
            // to use this list and the returned `Node` in the correct order.
            private Computation GenerateExpression(AST.Expression node, ILabel after)
            {
                switch (node)
                {
                    case AST.FunctionDeclaration expr:
                        return new Computation(after);
                    case AST.InstructionBlock expr:
                        return this.ConvertNode(expr, after);
                    case BlockWithResult expr:
                        return this.ConvertNode(expr, after);
                    case AST.VariableDeclaration expr:
                        return this.ConvertNode(expr, after);
                    case AST.WhileStatement expr:
                        return this.ConvertNode(expr, after);
                    case AST.IfStatement expr:
                        return this.ConvertNode(expr, after);
                    case AST.FunctionCall expr:
                        return this.ConvertNode(expr, after);
                    case AST.ReturnStatement expr:
                        return this.ConvertNode(expr, after);
                    case AST.BreakStatement expr:
                        return this.ConvertNode(expr, after);
                    case AST.ContinueStatement expr:
                        return this.ConvertNode(expr, after);
                    case AST.Variable expr:
                        return this.ConvertNode(expr, after);
                    case AST.BoolLiteral expr:
                        return this.ConvertNode(expr, after);
                    case AST.IntegerLiteral expr:
                        return this.ConvertNode(expr, after);
                    case AST.UnitLiteral expr:
                        return this.ConvertNode(expr, after);
                    case AST.NullLiteral expr:
                        return this.ConvertNode(expr, after);
                    case AST.Assignment expr:
                        return this.ConvertNode(expr, after);
                    case AST.CompoundAssignment expr:
                        return this.ConvertNode(expr, after);
                    case AST.ArithmeticOperation expr:
                        return this.ConvertNode(expr, after);
                    case AST.Comparison expr:
                        return this.ConvertNode(expr, after);
                    case AST.LogicalBinaryOperation expr:
                        return this.ConvertNode(expr, after);
                    case AST.UnaryOperation expr:
                        return this.ConvertNode(expr, after);
                    case AST.Nodes.IContainerAccess expr:
                        return this.ConvertNode(expr, after);
                    case AST.ComplexAssignment expr:
                        return this.ConvertNode(expr, after);
                    case AST.ComplexCompoundAssignment expr:
                        return this.ConvertNode(expr, after);
                    case AST.ArrayAlloc expr:
                        return this.ConvertNode(expr, after);
                    case AST.StructDeclaration expr:
                        return new Computation(after);
                    case AST.StructAlloc expr:
                        return this.ConvertNode(expr, after);
                    default:
                        throw new FunctionBodyGeneratorException($"Unknown node type: {node.Type}");
                }
            }

            private Computation ConvertNode(AST.InstructionBlock node, ILabel after)
            {
                ILabel start = node.Instructions
                    .Reverse()
                    .Aggregate(after, (next, expression) => this.GenerateExpression(expression, next).Start);

                return new Computation(start);
            }

            private Computation ConvertNode(BlockWithResult node, ILabel after)
            {
                Computation blockResult = this.GenerateExpression(node.Result, after);
                Computation block = this.ConvertNode(node.Body, blockResult.Start);

                return new Computation(block.Start, blockResult.Result);
            }

            private Computation ConvertNode(AST.VariableDeclaration node, ILabel after)
            {
                var result = this.labelFactory.WithLabel(
                    writeLabel =>
                    {
                        var nodeValue = node.Value ?? new AST.IntegerLiteral(node.InputRange, 0);
                        var value = this.GenerateExpression(nodeValue, writeLabel);
                        var write = this.readWriteGenerator.GenerateWrite(
                            this.function,
                            node.IntermediateVariable,
                            value.Result);
                        return (new Tree(write, new UnconditionalJump(after)), value);
                    });
                return new Computation(result.Start);
            }

            private Computation ConvertNode(AST.IfStatement node, ILabel after)
            {
                var thenBody = this.ConvertNode(node.ThenBody, after);
                var elseBody = this.ConvertNode(node.ElseBody, after);
                var result = this.labelFactory.WithLabel(
                    testLabel =>
                    {
                        var condition = this.GenerateExpression(node.Condition, testLabel);
                        return (new Tree(condition.Result, new ConditionalJump(thenBody.Start, elseBody.Start)),
                            condition);
                    });
                return new Computation(result.Start);
            }

            private Computation ConvertNode(AST.FunctionCall node, ILabel after)
            {
                var resultLocation = node.Declaration.ReturnType.IsHeapType()
                    ? (ILocation)this.function.ReserveStackFrameLocation(node.Declaration.ReturnType)
                    : new VirtualRegister();
                var argumentRegisters = Enumerable
                    .Range(0, node.Arguments.Count)
                    .Select(i => new VirtualRegister())
                    .ToList();
                var callArguments = argumentRegisters.ToList();
                callArguments.Reverse();
                var tempQualifier = node.Declaration.Function;
                var callLabel = this.callGenerator.GenerateCall(
                    resultLocation,
                    callArguments,
                    after,
                    this.function,
                    tempQualifier);

                var argumentsLabel = node.Arguments
                    .Reverse()
                    .Zip(argumentRegisters, (argument, register) => new { Argument = argument, Register = register })
                    .Aggregate(
                        callLabel,
                        (next, x) =>
                        {
                            var result = this.labelFactory.WithLabel(
                                writeLabel =>
                                {
                                    var argValue = this.GenerateExpression(x.Argument, writeLabel);
                                    Node write = new RegisterWrite(x.Register, argValue.Result);
                                    return (new Tree(write, new UnconditionalJump(next)), argValue);
                                });
                            return result.Start;
                        });

                return new Computation(
                    argumentsLabel,
                    this.readWriteGenerator.GenerateRead(this.function, resultLocation));
            }

            private Computation ConvertNode(AST.ReturnStatement node, ILabel after)
            {
                var result = this.labelFactory.WithLabel(
                    epilogueLabel =>
                    {
                        Computation value;
                        if (node.Value == null)
                        {
                            var resultNode = this.function.IsEntryPoint
                                ? (Node)new IntegerImmediateValue(0)
                                : new UnitImmediateValue();
                            value = new Computation(epilogueLabel, resultNode);
                        }
                        else
                        {
                            value = this.GenerateExpression(node.Value, epilogueLabel);
                        }

                        return (this.prologueEpilogueGenerator.GenerateEpilogue(this.function, value.Result).Tree,
                            value);
                    });
                return new Computation(result.Start);
            }

            private Computation ConvertNode(AST.WhileStatement node, ILabel after)
            {
                var condition = this.labelFactory.WithLabel(
                    testLabel =>
                    {
                        var innerCondition = this.GenerateExpression(node.Condition, testLabel);
                        this.loopLabels.Add(node, new LoopLabels(innerCondition.Start, after));
                        var body = this.ConvertNode(node.Body, innerCondition.Start);
                        var tree = new Tree(innerCondition.Result, new ConditionalJump(body.Start, after));
                        return (tree, innerCondition);
                    });
                return new Computation(condition.Start);
            }

            private Computation ConvertNode(AST.BreakStatement node, ILabel after)
            {
                return new Computation(this.loopLabels[node.EnclosingLoop].After);
            }

            private Computation ConvertNode(AST.ContinueStatement node, ILabel after)
            {
                return new Computation(this.loopLabels[node.EnclosingLoop].Condition);
            }

            private Computation ConvertNode(AST.Variable node, ILabel after)
            {
                var variable = node.Declaration.IntermediateVariable ?? throw new Exception("Variable is null.");
                var read = this.readWriteGenerator.GenerateRead(this.function, variable);
                return new Computation(after, read);
            }

            private Computation ConvertNode(AST.BoolLiteral node, ILabel after)
            {
                return new Computation(after, new BooleanImmediateValue(node.Value));
            }

            private Computation ConvertNode(AST.IntegerLiteral node, ILabel after)
            {
                return new Computation(after, new IntegerImmediateValue(node.Value));
            }

            private Computation ConvertNode(AST.UnitLiteral node, ILabel after)
            {
                return new Computation(after, new UnitImmediateValue());
            }

            private Computation ConvertNode(AST.NullLiteral node, ILabel after)
            {
                return new Computation(after, new IntegerImmediateValue(0));
            }

            private Computation ConvertNode(AST.Assignment node, ILabel after)
            {
                var variable = node.Lhs.Declaration.IntermediateVariable;
                var (value, readValue) = this.labelFactory.WithLabel(
                    copyLabel =>
                    {
                        var innerValue = this.GenerateExpression(node.Value, copyLabel);
                        var tmpRegister = new VirtualRegister();
                        var innerReadValue = new RegisterRead(tmpRegister);
                        var writeLabel = this.labelFactory.GetLabel(
                            new Tree(
                                this.readWriteGenerator.GenerateWrite(
                                    this.function,
                                    variable,
                                    innerReadValue),
                                new UnconditionalJump(after)));

                        return (new Tree(
                            new RegisterWrite(tmpRegister, innerValue.Result),
                            new UnconditionalJump(writeLabel)), (innerValue, innerReadValue));
                    });
                return new Computation(value.Start, readValue);
            }

            private Computation ConvertNode(AST.CompoundAssignment node, ILabel after)
            {
                return this.ConvertNode(this.SplitCompoundAssignment(node), after);
            }

            private AST.Assignment SplitCompoundAssignment(AST.CompoundAssignment node)
            {
                var operation = new AST.ArithmeticOperation(node.InputRange, node.Lhs, node.Value, node.Operation);
                return new AST.Assignment(node.InputRange, node.Lhs, operation);
            }

            private Computation ConvertNode(AST.ArithmeticOperation node, ILabel after)
            {
                Computation rhs = this.GenerateExpression(node.RightValue, after);
                Computation lhs = this.GenerateExpression(node.LeftValue, rhs.Start);
                Node result = new ArithmeticBinaryOperation(node.OperationType, lhs.Result, rhs.Result);
                return new Computation(lhs.Start, result);
            }

            private Computation ConvertNode(AST.Comparison node, ILabel after)
            {
                Computation rhs = this.GenerateExpression(node.RightValue, after);
                Computation lhs = this.GenerateExpression(node.LeftValue, rhs.Start);
                Node result = new Comparison(lhs.Result, rhs.Result, node.OperationType);
                return new Computation(lhs.Start, result);
            }

            private Computation ConvertNode(AST.LogicalBinaryOperation node, ILabel after)
            {
                var result = new VirtualRegister();
                var shortResultValue = node.BinaryOperationType == AST.LogicalBinaryOperationType.Or;
                Node writeShortResult = new RegisterWrite(result, new BooleanImmediateValue(shortResultValue));
                var shortLabel = this.labelFactory.GetLabel(new Tree(writeShortResult, new UnconditionalJump(after)));

                var rhs = this.labelFactory.WithLabel(
                    longLabel =>
                    {
                        var innerRhs = this.GenerateExpression(node.RightValue, longLabel);
                        return (new Tree(new RegisterWrite(result, innerRhs.Result), new UnconditionalJump(after)),
                            innerRhs);
                    });

                var lhs = this.labelFactory.WithLabel(
                    testLabel =>
                    {
                        var innerLhs = this.GenerateExpression(node.LeftValue, testLabel);
                        Tree tree;
                        switch (node.BinaryOperationType)
                        {
                            case AST.LogicalBinaryOperationType.And:
                                tree = new Tree(innerLhs.Result, new ConditionalJump(rhs.Start, shortLabel));
                                break;
                            case AST.LogicalBinaryOperationType.Or:
                                tree = new Tree(innerLhs.Result, new ConditionalJump(shortLabel, rhs.Start));
                                break;
                            default:
                                throw new Exception($"Unknown operation: {node.BinaryOperationType}");
                        }

                        return (tree, innerLhs);
                    });

                return new Computation(lhs.Start, new RegisterRead(result));
            }

            private Computation ConvertNode(AST.UnaryOperation node, ILabel after)
            {
                Computation operand = this.GenerateExpression(node.Value, after);
                Node result = new UnaryOperation(operand.Result, node.UnaryOperationType);
                return new Computation(operand.Start, result);
            }

            private Computation ContainerAccessMemoryLocation(AST.Nodes.IContainerAccess node, ILabel after)
            {
                var arrayAddressRegister = new VirtualRegister();
                var right = new VirtualRegister();

                var shiftValue = new ArithmeticBinaryOperation(
                    AST.ArithmeticOperationType.Multiplication,
                    new IntegerImmediateValue(8),
                    new RegisterRead(right));

                var root = new ArithmeticBinaryOperation(
                    AST.ArithmeticOperationType.Addition,
                    shiftValue,
                    new RegisterRead(arrayAddressRegister));

                var finalLabel = this.labelFactory.GetLabel(new Tree(root, new UnconditionalJump(after)));

                var rightLabel = this.labelFactory.WithLabel(
                    label =>
                    {
                        var rightComp = this.GenerateExpression(node.Offset, label);
                        var rightRoot = new RegisterWrite(right, rightComp.Result);
                        return (
                            new Tree(rightRoot, new UnconditionalJump(finalLabel)),
                            rightComp.Start);
                    });

                var leftLabel = this.labelFactory.WithLabel(
                    label =>
                    {
                        var leftComp = this.GenerateExpression(node.Lhs, label);
                        var leftRoot = new RegisterWrite(arrayAddressRegister, leftComp.Result);
                        return (
                            new Tree(leftRoot, new UnconditionalJump(rightLabel)),
                            leftComp.Start);
                    });

                return new Computation(leftLabel, root);
            }

            private Computation ContainerAccessMemoryLocation(AST.Expression expr, ILabel after)
            {
                const string errorMessage = "Incorrect access in ContainerAccessMemoryLocation";
                switch (expr)
                {
                    case AST.Nodes.IContainerAccess access:
                        return this.ContainerAccessMemoryLocation(access, after);

                    case BlockWithResult block:
                        var blockAccess = block.Result as AST.Nodes.IContainerAccess;
                        if (blockAccess == null)
                        {
                            throw new FunctionBodyGeneratorException(errorMessage);
                        }

                        var resultComputation = this.ContainerAccessMemoryLocation(blockAccess, after);
                        var bodyComputation = this.GenerateExpression(block.Body, resultComputation.Start);
                        return bodyComputation;

                    default:
                        Console.WriteLine($"default {expr}");
                        throw new FunctionBodyGeneratorException(errorMessage);
                }
            }

            private Computation ConvertNode(AST.Nodes.IContainerAccess node, ILabel after)
            {
                var addrRegister = new VirtualRegister();
                var addrRegisterValue = new RegisterRead(addrRegister);
                var root = new MemoryRead(addrRegisterValue);

                var finalLabel = this.labelFactory.GetLabel(new Tree(root, new UnconditionalJump(after)));

                var getAddrLabel = this.labelFactory.WithLabel(
                    label =>
                    {
                        var addrComputation = this.ContainerAccessMemoryLocation(node, label);
                        var addrRegisterWriteNode = new RegisterWrite(addrRegister, addrComputation.Result);
                        return (
                            new Tree(addrRegisterWriteNode, new UnconditionalJump(finalLabel)),
                            addrComputation.Start);
                    });

                return new Computation(getAddrLabel, root);
            }

            private Computation ConvertComplexAssignment(AST.Expression lhs, Func<Node, ILabel, Computation> value, ILabel after)
            {
                // ``value`` function should generate the value which needs to be written to Lhs. It is passed previous value of Lhs and the following label.
                var addrRegister = new VirtualRegister();
                var addrRegisterValue = new RegisterRead(addrRegister);

                var rhsRegister = new VirtualRegister();
                var rhsRegisterValue = new RegisterRead(rhsRegister);

                var root = new MemoryWrite(addrRegisterValue, rhsRegisterValue);
                var finalLabel = this.labelFactory.GetLabel(new Tree(root, new UnconditionalJump(after)));

                var rhsValueLabel = this.labelFactory.WithLabel(label =>
                {
                    var rhsValueComputation = value(new MemoryRead(new RegisterRead(addrRegister)), label);
                    var rhsRegisterValueWrite = new RegisterWrite(rhsRegister, rhsValueComputation.Result);
                    return (
                        new Tree(rhsRegisterValueWrite, new UnconditionalJump(finalLabel)),
                        rhsValueComputation.Start);
                });

                var getAddrLabel = this.labelFactory.WithLabel(label =>
                {
                    var addrComputation = this.ContainerAccessMemoryLocation(lhs, label);
                    var addrRegisterWriteNode = new RegisterWrite(addrRegister, addrComputation.Result);
                    return (
                        new Tree(addrRegisterWriteNode, new UnconditionalJump(rhsValueLabel)),
                        addrComputation.Start);
                });

                return new Computation(getAddrLabel, root);
            }

            private Computation ConvertNode(AST.ComplexAssignment node, ILabel after)
            {
                return this.ConvertComplexAssignment(node.Lhs, (prevValue, label) => this.GenerateExpression(node.Value, label), after);
            }

            private Computation ConvertNode(AST.ComplexCompoundAssignment node, ILabel after)
            {
                Func<Node, ILabel, Computation> value = (prevValue, label) =>
                {
                    Computation rhs = this.GenerateExpression(node.Value, label);
                    return new Computation(rhs.Start, new ArithmeticBinaryOperation(node.Operation, prevValue, rhs.Result));
                };
                return this.ConvertComplexAssignment(node.Lhs, value, after);
            }

            private Computation ConvertNode(AST.ArrayAlloc node, ILabel after)
            {
                var size = new AST.ArithmeticOperation(
                    node.InputRange,
                    new AST.IntegerLiteral(node.InputRange, 8),
                    node.Size,
                    AST.ArithmeticOperationType.Multiplication);

                var call = new AST.FunctionCall(
                    node.InputRange,
                    "allocate",
                    new List<AST.Expression> { size });

                var parameter = new AST.VariableDeclaration(
                    node.InputRange,
                    new AST.BuiltinTypes.IntType(),
                    "size",
                    null);

                var decl = new AST.FunctionDeclaration(
                    node.InputRange,
                    "allocate",
                    AST.Types.ArrayType.GetInstance(node.ElementType),
                    new List<AST.VariableDeclaration> { parameter },
                    null,
                    true);

                var mangledName = NameMangler.GetMangledName(decl, null);

                var func = new Function.Function(
                    null,
                    mangledName,
                    decl.Parameters,
                    decl.IsEntryPoint,
                    decl.IsForeign);

                decl.Function = func;

                call.Declaration = decl;
                return this.ConvertNode(call, after);
            }

            private Computation ConvertNode(AST.StructAlloc node, ILabel after)
            {
                var size = node.Declaration.Fields.Count * 8;

                var sizeLiteral = new AST.IntegerLiteral(node.InputRange, size);

                var call = new AST.FunctionCall(node.InputRange, "allocate", new List<AST.Expression> { sizeLiteral });

                var parameter = new AST.VariableDeclaration(
                    node.InputRange,
                    AST.BuiltinTypes.IntType.Instance,
                    "size",
                    null);

                var decl = new AST.FunctionDeclaration(
                    node.InputRange,
                    "allocate",
                    node.Type,
                    new List<AST.VariableDeclaration> { parameter },
                    null,
                    true);

                var mangledName = NameMangler.GetMangledName(decl, null);

                var func = new Function.Function(
                    null,
                    mangledName,
                    decl.Parameters,
                    decl.IsEntryPoint,
                    decl.IsForeign);

                decl.Function = func;

                call.Declaration = decl;
                return this.ConvertNode(call, after);
            }
        }
    }
}

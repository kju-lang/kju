namespace KJU.Core.Intermediate.FunctionBodyGenerator
{
    using System.Collections.Generic;
    using System.Linq;
    using TemporaryVariablesExtractor;

    public class FunctionBodyGenerator
    {
        private readonly Function function;

        private readonly Dictionary<AST.WhileStatement, LoopLabels> loopLabels =
            new Dictionary<AST.WhileStatement, LoopLabels>();

        public FunctionBodyGenerator(Function function)
        {
            this.function = function;
        }

        public Label BuildFunctionBody(AST.InstructionBlock body)
        {
            Label guardEpilogue = this.function.GenerateEpilogue(new UnitImmediateValue());
            Computation afterPrologue = this.ConvertNode(body, guardEpilogue);
            return this.function.GeneratePrologue(afterPrologue.Start);
        }

        // Any method that returns `Computation` prepends the created list
        // of `Tree`s (that begins at `Start`) to `after`. It's up to the caller
        // to use this list and the returned `Node` in the correct order.
        private Computation GenerateExpression(AST.Expression node, Label after)
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
                default:
                    throw new FunctionBodyGeneratorException($"Unknown node type: {node.Type}");
            }
        }

        private Computation ConvertNode(AST.InstructionBlock node, Label after)
        {
            Label start = node.Instructions
                .Reverse()
                .Aggregate(after, (next, expression) => this.GenerateExpression(expression, next).Start);

            return new Computation(start);
        }

        private Computation ConvertNode(BlockWithResult node, Label after)
        {
            Computation blockResult = this.GenerateExpression(node.Result, after);
            Computation block = this.ConvertNode(node.Body, blockResult.Start);

            return new Computation(block.Start, blockResult.Result);
        }

        private Computation ConvertNode(AST.VariableDeclaration node, Label after)
        {
            if (node.Value == null) return new Computation(after);

            Label writeLabel = new Label(null);
            Computation value = this.GenerateExpression(node.Value, writeLabel);
            Node write = this.function.GenerateWrite(node.IntermediateVariable, value.Result);
            writeLabel.Tree = new Tree(write, new UnconditionalJump(after));

            return new Computation(value.Start);
        }

        private Computation ConvertNode(AST.IfStatement node, Label after)
        {
            Computation thenBody = this.ConvertNode(node.ThenBody, after);
            Computation elseBody = this.ConvertNode(node.ElseBody, after);
            Label testLabel = new Label(null);

            Computation condition = this.GenerateExpression(node.Condition, testLabel);
            testLabel.Tree = new Tree(condition.Result, new ConditionalJump(thenBody.Start, elseBody.Start));
            return new Computation(condition.Start);
        }

        private Computation ConvertNode(AST.FunctionCall node, Label after)
        {
            Label callLabel = new Label(null);
            List<VirtualRegister> argumentRegisters = Enumerable
                .Range(0, node.Arguments.Count)
                .Select(i => new VirtualRegister())
                .ToList();

            var argumentsAndRegisters = node.Arguments.Reverse().Zip(
                argumentRegisters,
                (argument, register) => new { Argument = argument, Register = register });

            Label argumentsLabel = argumentsAndRegisters
                .Aggregate(callLabel, (next, x) =>
                {
                    Label writeLabel = new Label(null);
                    Computation argValue = this.GenerateExpression(x.Argument, writeLabel);
                    Node write = new RegisterWrite(x.Register, argValue.Result);
                    writeLabel.Tree = new Tree(write, new UnconditionalJump(next));
                    return argValue.Start;
                });

            argumentRegisters.Reverse();
            VirtualRegister resultRegister = new VirtualRegister();

            callLabel.Tree = node.Declaration.IntermediateFunction.GenerateCall(
                resultRegister,
                argumentRegisters,
                after,
                this.function).Tree;
            return new Computation(argumentsLabel, new RegisterRead(resultRegister));
        }

        private Computation ConvertNode(AST.ReturnStatement node, Label after)
        {
            Label epilogueLabel = new Label(null);
            Computation value = node.Value == null
                ? new Computation(epilogueLabel, new UnitImmediateValue())
                : this.GenerateExpression(node.Value, epilogueLabel);
            epilogueLabel.Tree = this.function.GenerateEpilogue(value.Result).Tree;
            return new Computation(value.Start);
        }

        private Computation ConvertNode(AST.WhileStatement node, Label after)
        {
            Label testLabel = new Label(null);
            Computation condition = this.GenerateExpression(node.Condition, testLabel);
            this.loopLabels.Add(node, new LoopLabels(condition.Start, after));

            Computation body = this.ConvertNode(node.Body, condition.Start);
            testLabel.Tree = new Tree(condition.Result, new ConditionalJump(body.Start, after));
            return new Computation(condition.Start);
        }

        private Computation ConvertNode(AST.BreakStatement node, Label after)
        {
            return new Computation(this.loopLabels[node.EnclosingLoop].After);
        }

        private Computation ConvertNode(AST.ContinueStatement node, Label after)
        {
            return new Computation(this.loopLabels[node.EnclosingLoop].Condition);
        }

        private Computation ConvertNode(AST.Variable node, Label after)
        {
            return new Computation(after, this.function.GenerateRead(node.Declaration.IntermediateVariable));
        }

        private Computation ConvertNode(AST.BoolLiteral node, Label after)
        {
            return new Computation(after, new BooleanImmediateValue(node.Value));
        }

        private Computation ConvertNode(AST.IntegerLiteral node, Label after)
        {
            return new Computation(after, new IntegerImmediateValue(node.Value));
        }

        private Computation ConvertNode(AST.UnitLiteral node, Label after)
        {
            return new Computation(after, new UnitImmediateValue());
        }

        private Computation ConvertNode(AST.Assignment node, Label after)
        {
            Variable variable = node.Lhs.Declaration.IntermediateVariable;
            Label copyLabel = new Label(null);
            Label writeLabel = new Label(null);

            Computation value = this.GenerateExpression(node.Value, copyLabel);
            VirtualRegister tmpRegister = new VirtualRegister();
            RegisterRead readValue = new RegisterRead(tmpRegister);

            copyLabel.Tree = new Tree(
                new RegisterWrite(tmpRegister, value.Result),
                new UnconditionalJump(writeLabel));

            writeLabel.Tree = new Tree(
                this.function.GenerateWrite(variable, readValue),
                new UnconditionalJump(after));

            return new Computation(value.Start, readValue);
        }

        private Computation ConvertNode(AST.CompoundAssignment node, Label after)
        {
            return this.ConvertNode(this.SplitCompoundAssignment(node), after);
        }

        private AST.Assignment SplitCompoundAssignment(AST.CompoundAssignment node)
        {
            var operation = new AST.ArithmeticOperation(node.Operation, node.Lhs, node.Value);
            return new AST.Assignment(node.Lhs, operation);
        }

        private Computation ConvertNode(AST.ArithmeticOperation node, Label after)
        {
            Computation rhs = this.GenerateExpression(node.RightValue, after);
            Computation lhs = this.GenerateExpression(node.LeftValue, rhs.Start);
            Node result = new ArithmeticBinaryOperation(node.OperationType, lhs.Result, rhs.Result);
            return new Computation(lhs.Start, result);
        }

        private Computation ConvertNode(AST.Comparison node, Label after)
        {
            Computation rhs = this.GenerateExpression(node.RightValue, after);
            Computation lhs = this.GenerateExpression(node.LeftValue, rhs.Start);
            Node result = new Comparison(lhs.Result, rhs.Result, node.OperationType);
            return new Computation(lhs.Start, result);
        }

        private Computation ConvertNode(AST.LogicalBinaryOperation node, Label after)
        {
            VirtualRegister result = new VirtualRegister();
            bool shortResultValue = node.BinaryOperationType == AST.LogicalBinaryOperationType.Or;
            Node writeShortResult = new RegisterWrite(result, new BooleanImmediateValue(shortResultValue));
            Label shortLabel = new Label(new Tree(writeShortResult, new UnconditionalJump(after)));

            Label longLabel = new Label(null);
            Computation rhs = this.GenerateExpression(node.RightValue, longLabel);
            longLabel.Tree = new Tree(new RegisterWrite(result, rhs.Result), new UnconditionalJump(after));

            Label testLabel = new Label(null);
            Computation lhs = this.GenerateExpression(node.LeftValue, testLabel);

            switch (node.BinaryOperationType)
            {
                case AST.LogicalBinaryOperationType.And:
                    testLabel.Tree = new Tree(lhs.Result, new ConditionalJump(rhs.Start, shortLabel));
                    break;
                case AST.LogicalBinaryOperationType.Or:
                    testLabel.Tree = new Tree(lhs.Result, new ConditionalJump(shortLabel, rhs.Start));
                    break;
            }

            return new Computation(lhs.Start, new RegisterRead(result));
        }

        private Computation ConvertNode(AST.UnaryOperation node, Label after)
        {
            Computation operand = this.GenerateExpression(node.Value, after);
            Node result = new UnaryOperation(operand.Result, node.UnaryOperationType);
            return new Computation(operand.Start, result);
        }
    }
}
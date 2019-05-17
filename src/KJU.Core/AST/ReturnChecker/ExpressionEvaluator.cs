namespace KJU.Core.AST.ReturnChecker
{
    public static class ExpressionEvaluator
    {
        public static Expression PartiallyEvaluate(this Expression node)
        {
            switch (node)
            {
                case UnaryOperation op:
                    switch (op.UnaryOperationType)
                    {
                        case UnaryOperationType.Minus:
                        {
                            var val = op.Value.AsInteger();
                            if (val == null)
                            {
                                return op;
                            }

                            return new IntegerLiteral(node.InputRange, -val.Value);
                        }

                        case UnaryOperationType.Plus:
                        {
                            var val = op.Value.AsInteger();
                            if (val == null)
                            {
                                return op;
                            }

                            return new IntegerLiteral(node.InputRange, val.Value);
                        }

                        case UnaryOperationType.Not:
                        {
                            var val = op.Value.AsBool();
                            if (val == null)
                            {
                                return op;
                            }

                            return new BoolLiteral(node.InputRange, !val.Value);
                        }

                        default:
                            throw new ExpressionEvaluatorException(
                                $"Unknown unary operation of type: {op.UnaryOperationType}. This should never happen.");
                    }

                case ArithmeticOperation op:
                {
                    var left = op.LeftValue.AsInteger();
                    var right = op.RightValue.AsInteger();

                    if (left == null || right == null)
                    {
                        return op;
                    }

                    switch (op.OperationType)
                    {
                        case ArithmeticOperationType.Addition:
                            return new IntegerLiteral(node.InputRange, left.Value + right.Value);
                        case ArithmeticOperationType.Subtraction:
                            return new IntegerLiteral(node.InputRange, left.Value - right.Value);
                        case ArithmeticOperationType.Multiplication:
                            return new IntegerLiteral(node.InputRange, left.Value * right.Value);
                        case ArithmeticOperationType.Division:
                            if (right.Value == 0)
                            {
                                return null;
                            }

                            return new IntegerLiteral(node.InputRange, left.Value / right.Value);
                        case ArithmeticOperationType.Remainder:
                            if (right.Value == 0)
                            {
                                return null;
                            }

                            return new IntegerLiteral(node.InputRange, left.Value % right.Value);
                        default:
                            throw new ExpressionEvaluatorException(
                                $"Unknown arithmetic operation of type: {op.OperationType}. This should never happen.");
                    }
                }

                case Comparison op:
                {
                    var left = op.LeftValue.AsInteger();
                    var right = op.RightValue.AsInteger();

                    if (left == null || right == null)
                    {
                        return op;
                    }

                    switch (op.OperationType)
                    {
                        case ComparisonType.Equal:
                            return new BoolLiteral(node.InputRange, left.Value == right.Value);
                        case ComparisonType.NotEqual:
                            return new BoolLiteral(node.InputRange, left.Value != right.Value);
                        case ComparisonType.Less:
                            return new BoolLiteral(node.InputRange, left.Value < right.Value);
                        case ComparisonType.LessOrEqual:
                            return new BoolLiteral(node.InputRange, left.Value <= right.Value);
                        case ComparisonType.Greater:
                            return new BoolLiteral(node.InputRange, left.Value > right.Value);
                        case ComparisonType.GreaterOrEqual:
                            return new BoolLiteral(node.InputRange, left.Value >= right.Value);
                        default:
                            throw new ExpressionEvaluatorException(
                                $"Unknown comparision operation of type: {op.OperationType}. This should never happen.");
                    }
                }

                case LogicalBinaryOperation op:
                {
                    var left = op.LeftValue.AsBool();
                    if (left == null)
                    {
                        return op.RightValue.PartiallyEvaluate();
                    }

                    switch (op.BinaryOperationType)
                    {
                        case LogicalBinaryOperationType.And:
                            return left.Value
                                ? op.RightValue.PartiallyEvaluate()
                                : new BoolLiteral(node.InputRange, false);
                        case LogicalBinaryOperationType.Or:
                            return left.Value
                                ? new BoolLiteral(node.InputRange, true)
                                : op.RightValue.PartiallyEvaluate();
                        default:
                            throw new ExpressionEvaluatorException(
                                $"Unknown logical binary operation of type: {op.BinaryOperationType}. This should never happen.");
                    }
                }
            }

            return node;
        }

        public static long? AsInteger(this Expression node)
        {
            switch (node.PartiallyEvaluate())
            {
                case IntegerLiteral lit:
                    return lit.Value;
                default:
                    return null;
            }
        }

        public static bool? AsBool(this Expression node)
        {
            switch (node.PartiallyEvaluate())
            {
                case BoolLiteral lit:
                    return lit.Value;
                default:
                    return null;
            }
        }
    }
}
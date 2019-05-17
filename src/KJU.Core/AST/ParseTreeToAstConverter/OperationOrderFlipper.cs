namespace KJU.Core.AST.ParseTreeToAstConverter
{
    using System.Collections.Generic;

    internal class OperationOrderFlipper : IOperationOrderFlipper
    {
        private readonly HashSet<Node> enclosedWithParentheses = new HashSet<Node>();

        public void AddEnclosedWithParentheses(Node node)
        {
            this.enclosedWithParentheses.Add(node);
        }

        public void FlipToLeftAssignmentAst(Node ast)
        {
            var arithmeticOrder = new Dictionary<ArithmeticOperationType, int>
            {
                [ArithmeticOperationType.Addition] = 0,
                [ArithmeticOperationType.Subtraction] = 0,
                [ArithmeticOperationType.Multiplication] = 1,
                [ArithmeticOperationType.Division] = 1,
                [ArithmeticOperationType.Remainder] = 1,
            };

            if (this.enclosedWithParentheses.Contains(ast))
            {
                this.enclosedWithParentheses.Remove(ast);
            }

            if (ast is BinaryOperation root)
            {
                var path = new List<BinaryOperation>();
                var danglingNodes = new List<Expression>();
                var current = root;
                while (current.GetType() == root.GetType())
                {
                    if (this.enclosedWithParentheses.Contains(current))
                    {
                        break;
                    }

                    if (current is ArithmeticOperation currentOp && root is ArithmeticOperation rootOp)
                    {
                        if (arithmeticOrder[currentOp.OperationType] != arithmeticOrder[rootOp.OperationType])
                        {
                            break;
                        }
                    }

                    path.Add(current);
                    this.FlipToLeftAssignmentAst(current.LeftValue);
                    danglingNodes.Add(current.LeftValue);
                    if (!(current.RightValue is BinaryOperation))
                    {
                        break;
                    }

                    current = (BinaryOperation)current.RightValue;
                }

                var n = path.Count;
                this.FlipToLeftAssignmentAst(path[n - 1].RightValue);
                danglingNodes.Add(path[n - 1].RightValue);

                // path[0] == root
                for (var i = 0; i < n - 1; i++)
                {
                    path[i].LeftValue = path[i + 1];
                    path[i].RightValue = danglingNodes[n - i];
                }

                path[n - 1].LeftValue = danglingNodes[0];
                path[n - 1].RightValue = danglingNodes[1];
                SwapOperationsInPath(path);
            }
            else
            {
                foreach (var child in ast.Children())
                {
                    this.FlipToLeftAssignmentAst(child);
                }
            }
        }

        private static void SwapOps(BinaryOperation first, BinaryOperation second)
        {
            switch (first)
            {
                case ArithmeticOperation firstArithmeticOperation:
                    var arithmeticType = firstArithmeticOperation.OperationType;
                    var secondArithmeticOperation = (ArithmeticOperation)second;
                    firstArithmeticOperation.OperationType = secondArithmeticOperation.OperationType;
                    secondArithmeticOperation.OperationType = arithmeticType;
                    break;
                case Comparison firstComparision:
                    var comparisonType = firstComparision.OperationType;
                    var secondComparision = (Comparison)second;
                    firstComparision.OperationType = secondComparision.OperationType;
                    secondComparision.OperationType = comparisonType;
                    break;
                case LogicalBinaryOperation firstLogicOperation:
                    var logicType = firstLogicOperation.BinaryOperationType;
                    var secondLogicOperation = (LogicalBinaryOperation)second;
                    firstLogicOperation.BinaryOperationType = secondLogicOperation.BinaryOperationType;
                    secondLogicOperation.BinaryOperationType = logicType;
                    break;
            }
        }

        private static void SwapOperationsInPath(IReadOnlyList<BinaryOperation> path)
        {
            var n = path.Count;
            for (var i = 0; i < n / 2; i++)
            {
                SwapOps(path[i], path[n - (i + 1)]);
            }
        }
    }
}
namespace KJU.Tests.AST
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using KJU.Core.AST;
    using KJU.Core.AST.BuiltinTypes;
    using KJU.Core.AST.ParseTreeToAstConverter;
    using KJU.Core.AST.Types;
    using KJU.Core.Diagnostics;
    using KJU.Core.Lexer;
    using KJU.Core.Parser;
    using KJU.Tests.Util;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class KjuParseTreeToAstConverterTest
    {
        private readonly Diagnostics diagnostics;
        private readonly Dictionary<ComparisonType, string> comparisionTypeToString;
        private readonly Dictionary<ArithmeticOperationType, string> arithmeticOperationTypeToString;
        private readonly Dictionary<LogicalBinaryOperationType, string> logicalBinaryOperationTypeToString;
        private readonly Dictionary<UnaryOperationType, string> logicalUnaryOperationTypeToString;

        public KjuParseTreeToAstConverterTest()
        {
            this.diagnostics = new Mock<Diagnostics>().Object;

            this.arithmeticOperationTypeToString = new Dictionary<ArithmeticOperationType, string>()
            {
                [ArithmeticOperationType.Addition] = "+",
                [ArithmeticOperationType.Division] = "/",
                [ArithmeticOperationType.Subtraction] = "-",
                [ArithmeticOperationType.Remainder] = "%",
                [ArithmeticOperationType.Multiplication] = "*",
            };

            this.comparisionTypeToString = new Dictionary<ComparisonType, string>()
            {
                [ComparisonType.Equal] = "==",
                [ComparisonType.NotEqual] = "!=",
                [ComparisonType.Less] = "<",
                [ComparisonType.LessOrEqual] = "<=",
                [ComparisonType.Greater] = ">",
                [ComparisonType.GreaterOrEqual] = ">=",
            };

            this.logicalBinaryOperationTypeToString = new Dictionary<LogicalBinaryOperationType, string>()
            {
                [LogicalBinaryOperationType.Or] = "or",
                [LogicalBinaryOperationType.And] = "and",
            };
            this.logicalUnaryOperationTypeToString = new Dictionary<UnaryOperationType, string>()
            {
                [UnaryOperationType.Not] = "!",
                [UnaryOperationType.Plus] = "+",
                [UnaryOperationType.Minus] = "-",
            };
        }

        [TestMethod]
        public void VariableDeclaration()
        {
            string code = @"
                fun kju ( param : Int ) : Unit {
                    var variable : Bool ;
                }";
            var expected = "P<fun kju Unit<var param Int<> | block<var variable Bool<>>>>";
            this.TestTemplate(code, expected);
        }

        [TestMethod]
        public void MultipleFunctions()
        {
            string code = @"
                fun fun1 ( ) : Unit { }
                fun fun2 ( ) : Int { }
                fun fun3 ( ) : Bool { }
                ";
            string expected = "P<fun fun1 Unit<block<>> | fun fun2 Int<block<>> | fun fun3 Bool<block<>>>";
            this.TestTemplate(code, expected);
        }

        [TestMethod]
        public void FunctionCall()
        {
            string code = @"
                fun kju ( ) : Unit {
                    abc ( 15 ) ;
                }";
            string expected = "P<fun kju Unit<block<call abc<int 15>>>>";
            this.TestTemplate(code, expected);
        }

        [TestMethod]
        public void ReturnStatement()
        {
            string code = @"
                fun kju ( ) : Unit {
                    return ;
                    return 15 ;
                }";
            string expected = "P<fun kju Unit<block<return<> | return<int 15>>>>";
            this.TestTemplate(code, expected);
        }

        [TestMethod]
        public void BreakStatement()
        {
            string code = @"
                fun kju ( ) : Unit {
                    break ;
                }";
            string expected = "P<fun kju Unit<block<break>>>";
            this.TestTemplate(code, expected);
        }

        [TestMethod]
        public void ContinueStatement()
        {
            string code = @"
                fun kju ( ) : Unit {
                    continue ;
                }";
            string expected = "P<fun kju Unit<block<continue>>>";
            this.TestTemplate(code, expected);
        }

        [TestMethod]
        public void VariableTest()
        {
            string code = @"
                fun kju ( ) : Unit {
                    x ;
                }";
            string expected = "P<fun kju Unit<block<var x>>>";
            this.TestTemplate(code, expected);
        }

        [TestMethod]
        public void IfStatement()
        {
            string code = @"
                fun kju ( ) : Unit {
                    if ( x == 15 ) then { 1 ; } else { } ;
                }";
            string expected = "P<fun kju Unit<block<if<cmp ==<var x | int 15> | block<int 1> | block<>>>>>";
            this.TestTemplate(code, expected);
        }

        [TestMethod]
        public void WhileStatement()
        {
            string code = @"
                fun kju ( ) : Unit {
                    while ( true ) {  } ;
                }";
            string expected = "P<fun kju Unit<block<while<bool True | block<>>>>>";
            this.TestTemplate(code, expected);
        }

        [TestMethod]
        public void Assignment()
        {
            string code = @"
                fun kju ( ) : Unit {
                    x = 15 ;
                }";
            string expected = "P<fun kju Unit<block<assign<var x | int 15>>>>";
            this.TestTemplate(code, expected);
        }

        [TestMethod]
        public void CompoundAssignment()
        {
            string code = @"
                fun kju ( ) : Unit {
                    x -= 15 ;
                }";
            string expected = "P<fun kju Unit<block<assign -<var x | int 15>>>>";
            this.TestTemplate(code, expected);
        }

        [TestMethod]
        public void OperationsLeftAssignment()
        {
            string code = @"
                fun kju ( ) : Unit {
                    x + y + z ;
                }";
            string expected = "P<fun kju Unit<block<op +<op +<var x | var y> | var z>>>>";
            this.TestTemplate(code, expected);
        }

        [TestMethod]
        public void OperationsLeftAssignmentMixedOps()
        {
            string code = @"
                fun kju ( ) : Unit {
                    x - y + z ;
                }";
            string expected = "P<fun kju Unit<block<op +<op -<var x | var y> | var z>>>>";
            this.TestTemplate(code, expected);
        }

        [TestMethod]
        public void OperationsPriority()
        {
            string code = @"
                fun kju ( ) : Unit {
                    x + y * z ;
                }";
            string expected = "P<fun kju Unit<block<op +<var x | op *<var y | var z>>>>>";
            this.TestTemplate(code, expected);
        }

        [TestMethod]
        public void OperationsLeftAssignmentParenthesis()
        {
            var code = @"
            fun kju ( ) : Unit {
                x + ( y + z ) ;
            }";
            var expected = "P<fun kju Unit<block<op +<var x | op +<var y | var z>>>>>";
            this.TestTemplate(code, expected);
        }

        [TestMethod]
        public void OperationsOrderMixedPrecedenceAndParenthesis()
        {
            var code = @"
            fun kju ( ) : Unit {
                (x +  y) * z ;
            }";
            var expected = "P<fun kju Unit<block<op *<op +<var x | var y> | var z>>>>";
            this.TestTemplate(code, expected);
        }

        [TestMethod]
        public void OperationsLeftAssignmentParenthesisAlternatingOperators()
        {
            var code = @"
            fun kju ( ) : Unit {
                x - ( y + z ) ;
            }";
            var expected = "P<fun kju Unit<block<op -<var x | op +<var y | var z>>>>>";
            this.TestTemplate(code, expected);
        }

        [TestMethod]
        public void Comparision()
        {
            string code = @"
                fun kju ( ) : Unit {
                    x <= 10 ;
                }";
            string expected = "P<fun kju Unit<block<cmp <=<var x | int 10>>>>";
            this.TestTemplate(code, expected);
        }

        [TestMethod]
        public void LogicalOperation()
        {
            string code = @"
                fun kju ( ) : Unit {
                    true && false ;
                }";
            string expected = "P<fun kju Unit<block<logic and<bool True | bool False>>>>";
            this.TestTemplate(code, expected);
        }

        [TestMethod]
        public void LogicalOperationOrder()
        {
            string code = @"
                fun kju ( ) : Unit {
                    true || false || true ;
                }";
            string expected = "P<fun kju Unit<block<logic or<logic or<bool True | bool False> | bool True>>>>";
            this.TestTemplate(code, expected);
        }

        [TestMethod]
        public void LogicalOperationNot()
        {
            string code = @"
                fun kju ( ) : Unit {
                    ! true ;
                }";
            string expected = "P<fun kju Unit<block<unary !<bool True>>>>";
            this.TestTemplate(code, expected);
        }

        [TestMethod]
        public void LogicalOperationDoubleNot()
        {
            string code = @"
                fun kju ( ) : Unit {
                   ! ! true ;
                }";
            string expected = "P<fun kju Unit<block<unary !<unary !<bool True>>>>>";
            this.TestTemplate(code, expected);
        }

        [TestMethod]
        public void UnaryArithmeticOperationsNot()
        {
            var code = @"
                fun kju ( ) : Unit {
                   -x;
                }";
            var expected = "P<fun kju Unit<block<unary -<var x>>>>";
            this.TestTemplate(code, expected);
        }

        [TestMethod]
        public void VariableDeclarationWithExpression()
        {
            string code = @"
                fun kju ( ) : Unit {
                   var x : Int = 2 + 1  ;
                }";
            string expected = "P<fun kju Unit<block<var x Int<op +<int 2 | int 1>>>>>";
            this.TestTemplate(code, expected);
        }

        [TestMethod]
        public void ComparisonOrder()
        {
            string code = @"
                fun kju ( ) : Unit {
                   true == false == true  ;
                }";
            string expected = "P<fun kju Unit<block<cmp ==<cmp ==<bool True | bool False> | bool True>>>>";
            this.TestTemplate(code, expected);
        }

        [TestMethod]
        public void AssignmentOrder()
        {
            string code = @"
                fun kju (): Unit {
                    var x: Int;
                    var y: Int = 0;
                    x = y + 3;
                }
                ";
            string expected = "P<fun kju Unit<block<var x Int<> | var y Int<int 0> | assign<var x | op +<var y | int 3>>>>>";
            this.TestTemplate(code, expected);
        }

        [TestMethod]
        public void CompoundAssignmentOrder()
        {
            string code = @"
                fun kju (): Unit {
                    var x: Int;
                    x += x + 3;
                }
                ";
            string expected = "P<fun kju Unit<block<var x Int<> | assign +<var x | op +<var x | int 3>>>>>";
            this.TestTemplate(code, expected);
        }

        [TestMethod]
        public void MultipleAssignments()
        {
            string code = @"
                fun kju (): Unit {
                    var x: Int;
                    var y: Int;
                    x = y = 3 + (y = 0);
                }
                ";
            string expected = "P<fun kju Unit<block<var x Int<> | var y Int<> | assign<var x | assign<var y | op +<int 3 | assign<var y | int 0>>>>>>>";
            this.TestTemplate(code, expected);
        }

        [TestMethod]
        public void AssignmentLhsNotVariable()
        {
            var diag = new Mock<IDiagnostics>();
            string code = @"
                fun kju (): Unit {
                    4 * 6 = 5;
                }
                ";
            Assert.ThrowsException<ParseTreeToAstConverterException>(
                () => KjuCompilerUtils.GenerateAst(code, diag.Object));
            MockDiagnostics.Verify(diag, KjuParseTreeToAstConverter.AssignmentLhsErrorDiagnosticsType);
        }

        [TestMethod]
        public void ArrayCreate()
        {
            string code = @"
                fun kju(): Unit {
                    var x : [[[Int]]];
                }
                ";
            string expected = "P<fun kju Unit<block<var x [[[Int]]]<>>>>";
            this.TestTemplate(code, expected);
        }

        [TestMethod]
        public void ArrayDeclarationWithDefinition()
        {
            string code = @"
                fun kju(): Unit {
                    var x : [[[Int]]] = new ([[Int]], 5);
                }
                ";
            string expected = "P<fun kju Unit<block<var x [[[Int]]]<alloc [[Int]]<int 5>>>>>";
            this.TestTemplate(code, expected);
        }

        [TestMethod]
        public void ArrayAccessOneDim()
        {
            string code = @"
                fun kju(): Unit {
                    x[1];
                }
                ";
            string expected = "P<fun kju Unit<block<access<var x | int 1>>>>";
            this.TestTemplate(code, expected);
        }

        [TestMethod]
        public void ArrayAccessMultiDim()
        {
            string code = @"
                fun kju(): Unit {
                    x[1][2][3];
                }
                ";
            string expected = "P<fun kju Unit<block<access<access<access<var x | int 1> | int 2> | int 3>>>>";
            this.TestTemplate(code, expected);
        }

        [TestMethod]
        public void ArrayAssignmentOneDim()
        {
            string code = @"
                fun kju(): Unit {
                    x[1] = 2;
                }
                ";
            string expected = "P<fun kju Unit<block<array_assign<access<var x | int 1> | int 2>>>>";
            this.TestTemplate(code, expected);
        }

        [TestMethod]
        public void ArrayAssignmentMultiDim()
        {
            string code = @"
                fun kju(): Unit {
                    x[1][2][3] = y[4];
                }
                ";
            string expected = "P<fun kju Unit<block<array_assign<access<access<access<var x | int 1> | int 2> | int 3> | access<var y | int 4>>>>>";
            this.TestTemplate(code, expected);
        }

        [TestMethod]
        public void ArrayCompoundAssignmentOneDim()
        {
            string code = @"
                fun kju(): Unit {
                    x[1] += 2;
                }
                ";
            string expected = "P<fun kju Unit<block<array_assign +<access<var x | int 1> | int 2>>>>";
            this.TestTemplate(code, expected);
        }

        [TestMethod]
        public void ArrayCompoundAssignmentMultiDim()
        {
            string code = @"
                fun kju(): Unit {
                    x[1][2][3] %= y[4];
                }
                ";
            string expected = "P<fun kju Unit<block<array_assign %<access<access<access<var x | int 1> | int 2> | int 3> | access<var y | int 4>>>>>";
            this.TestTemplate(code, expected);
        }

        [TestMethod]
        public void FunctionReturnNew()
        {
            string code = @"
                fun f() : [Int] {
                    return new (Int, 5);
                }
                ";
            string expected = "P<fun f [Int]<block<return<alloc Int<int 5>>>>>";
            this.TestTemplate(code, expected);
        }

        [TestMethod]
        public void FunctionAccess()
        {
            string code = @"
                fun f() : [Int] {
                    return new (Int, 5);
                }
                fun kju(): Unit {
                    x[1][2][3] *= f()[4];
                }
                ";
            string expected = "P<fun f [Int]<block<return<alloc Int<int 5>>>> | fun kju Unit<block<array_assign *<access<access<access<var x | int 1> | int 2> | int 3> | access<call f<> | int 4>>>>>";
            this.TestTemplate(code, expected);
        }

        private void TestTemplate(string code, string expectedAstSerialization)
        {
            var ast = KjuCompilerUtils.GenerateAst(code, this.diagnostics);

            Assert.AreEqual(expectedAstSerialization, this.SerializeAst(ast));
        }

        private string SerializeAst(Node ast)
        {
            var builder = new StringBuilder();
            this.SerializeAst(ast, builder);
            return builder.ToString();
        }

        private void SerializeAst(Node ast, StringBuilder builder)
        {
            Assert.IsNotNull(ast.InputRange, $"Input range of ast node {ast} is not set");
            switch (ast)
            {
                case Program _:
                    builder.Append("P");
                    break;
                case FunctionDeclaration functionDeclaration:
                    var retType = this.DataTypeToString(functionDeclaration.ReturnType);
                    builder.Append($"fun {functionDeclaration.Identifier} {retType}");
                    break;
                case InstructionBlock _:
                    builder.Append("block");
                    break;
                case VariableDeclaration variableDeclaration:
                    var type = this.DataTypeToString(variableDeclaration.VariableType);
                    builder.Append($"var {variableDeclaration.Identifier} {type}");
                    break;
                case WhileStatement _:
                    builder.Append("while");
                    break;
                case IfStatement _:
                    builder.Append("if");
                    break;
                case FunctionCall functionCall:
                    builder.Append($"call {functionCall.Identifier}");
                    break;
                case ReturnStatement _:
                    builder.Append("return");
                    break;
                case BreakStatement _:
                    builder.Append("break");
                    return;
                case ContinueStatement _:
                    builder.Append("continue");
                    return;
                case Variable variable:
                    builder.Append($"var {variable.Identifier}");
                    return;
                case BoolLiteral boolLiteral:
                    builder.Append($"bool {boolLiteral.Value}");
                    return;
                case IntegerLiteral integerLiteral:
                    builder.Append($"int {integerLiteral.Value}");
                    return;
                case Assignment _:
                    builder.Append($"assign");
                    break;
                case CompoundAssignment compoundAssignment:
                    var op = this.arithmeticOperationTypeToString[compoundAssignment.Operation];
                    builder.Append($"assign {op}");
                    break;
                case ComplexAssignment _:
                    builder.Append($"array_assign");
                    break;
                case ComplexCompoundAssignment compoundAssignment:
                    var arrayOp = this.arithmeticOperationTypeToString[compoundAssignment.Operation];
                    builder.Append($"array_assign {arrayOp}");
                    break;
                case ArrayAlloc alloc:
                    builder.Append($"alloc {this.DataTypeToString(alloc.ElementType)}");
                    break;
                case ArrayAccess access:
                    builder.Append($"access");
                    break;
                case ArithmeticOperation arithmeticOperation:
                    op = this.arithmeticOperationTypeToString[arithmeticOperation.OperationType];
                    builder.Append($"op {op}");
                    break;
                case Comparison comparison:
                    op = this.comparisionTypeToString[comparison.OperationType];
                    builder.Append($"cmp {op}");
                    break;
                case LogicalBinaryOperation logicalOperation:
                    op = this.logicalBinaryOperationTypeToString[logicalOperation.BinaryOperationType];
                    builder.Append($"logic {op}");
                    break;
                case UnaryOperation unaryOperation:
                    op = this.logicalUnaryOperationTypeToString[unaryOperation.UnaryOperationType];
                    builder.Append($"unary {op}");
                    break;

                default:
                    throw new Exception("unexpected type");
            }

            builder.Append("<");
            var firstChild = true;
            foreach (var child in ast.Children())
            {
                if (firstChild)
                {
                    firstChild = false;
                }
                else
                {
                    builder.Append(" | ");
                }

                this.SerializeAst(child, builder);
            }

            builder.Append(">");
        }

        private string DataTypeToString(DataType type)
        {
            switch (type)
            {
                case BoolType _:
                    return "Bool";
                case IntType _:
                    return "Int";
                case UnitType _:
                    return "Unit";
                case ArrayType arrayType:
                    return $"[{this.DataTypeToString(arrayType.ElementType)}]";
                default:
                    throw new Exception("Incorrect DataType");
            }
        }
    }
}
#pragma warning disable SA1008 // Opening parenthesis must not be preceded by a space
#pragma warning disable SA1009 // Closing parenthesis must not be followed by a space
namespace KJU.Tests.AST
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using KJU.Core.AST;
    using KJU.Core.AST.BuiltinTypes;
    using KJU.Core.AST.TypeChecker;
    using KJU.Core.AST.Types;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Util;

    [TestClass]
    public class SolverTests
    {
        [TestMethod]
        public void TestSimple()
        {
            var typeVar = new TypeVariable();
            var intVar = new IntType();
            var clauses = new List<Clause>
            {
                new Clause
                {
                    Alternatives = new List<List<(IHerbrandObject, IHerbrandObject)>>
                    {
                        new List<(IHerbrandObject, IHerbrandObject)>
                        {
                            (typeVar, intVar)
                        }
                    }
                }
            };

            var solver = new Solver(clauses);
            var expectedSolution = new Dictionary<TypeVariable, IHerbrandObject>
            {
                { typeVar, intVar }
            };
            CheckSolution(solver, expectedSolution);
        }

        [TestMethod]
        public void TestBacktrack()
        {
            var xTypeVar = new TypeVariable();
            var yTypeVar = new TypeVariable();

            var intVar = IntType.Instance;
            var boolVar = BoolType.Instance;

            var clauses = new List<Clause>
            {
                new Clause
                {
                    Alternatives = new List<List<(IHerbrandObject, IHerbrandObject)>>
                    {
                        new List<(IHerbrandObject, IHerbrandObject)> { (xTypeVar, intVar) },
                        new List<(IHerbrandObject, IHerbrandObject)> { (xTypeVar, boolVar) }
                    }
                },
                new Clause
                {
                    Alternatives = new List<List<(IHerbrandObject, IHerbrandObject)>>
                    {
                        new List<(IHerbrandObject, IHerbrandObject)> { (yTypeVar, intVar) },
                        new List<(IHerbrandObject, IHerbrandObject)> { (yTypeVar, boolVar) }
                    }
                },
                new Clause
                {
                    Alternatives = new List<List<(IHerbrandObject, IHerbrandObject)>>
                    {
                        new List<(IHerbrandObject, IHerbrandObject)>
                        {
                            (xTypeVar, boolVar),
                            (yTypeVar, intVar)
                        }
                    }
                }
            };

            var solver = new Solver(clauses);
            var expectedSolution = new Dictionary<TypeVariable, IHerbrandObject>
            {
                { xTypeVar, boolVar },
                { yTypeVar, intVar }
            };
            CheckSolution(solver, expectedSolution);
        }

        [TestMethod]
        public void TestNoSolution()
        {
            var xTypeVar = new TypeVariable();
            var yTypeVar = new TypeVariable();

            var intVar = IntType.Instance;
            var boolVar = BoolType.Instance;
            var unitVar = UnitType.Instance;

            var clauses = new List<Clause>
            {
                new Clause
                {
                    Alternatives = new List<List<(IHerbrandObject, IHerbrandObject)>>
                    {
                        new List<(IHerbrandObject, IHerbrandObject)> { (xTypeVar, intVar) },
                        new List<(IHerbrandObject, IHerbrandObject)> { (xTypeVar, boolVar) }
                    }
                },
                new Clause
                {
                    Alternatives = new List<List<(IHerbrandObject, IHerbrandObject)>>
                    {
                        new List<(IHerbrandObject, IHerbrandObject)> { (yTypeVar, intVar) },
                        new List<(IHerbrandObject, IHerbrandObject)> { (yTypeVar, boolVar) }
                    }
                },
                new Clause
                {
                    Alternatives = new List<List<(IHerbrandObject, IHerbrandObject)>>
                    {
                        new List<(IHerbrandObject, IHerbrandObject)>
                        {
                            (xTypeVar, unitVar),
                            (yTypeVar, unitVar)
                        }
                    }
                }
            };

            var solver = new Solver(clauses);
            CheckException(solver);
        }

        [TestMethod]
        public void TestMultipleSolutions()
        {
            var xTypeVar = new TypeVariable();
            var yTypeVar = new TypeVariable();

            var intVar = IntType.Instance;
            var boolVar = BoolType.Instance;

            var clauses = new List<Clause>
            {
                new Clause
                {
                    Alternatives = new List<List<(IHerbrandObject, IHerbrandObject)>>
                    {
                        new List<(IHerbrandObject, IHerbrandObject)> { (xTypeVar, intVar) },
                        new List<(IHerbrandObject, IHerbrandObject)> { (xTypeVar, boolVar) }
                    }
                },
                new Clause
                {
                    Alternatives = new List<List<(IHerbrandObject, IHerbrandObject)>>
                    {
                        new List<(IHerbrandObject, IHerbrandObject)> { (yTypeVar, intVar) },
                        new List<(IHerbrandObject, IHerbrandObject)> { (yTypeVar, boolVar) }
                    }
                }
            };

            var solver = new Solver(clauses);
            CheckException(solver);
        }

        [TestMethod]
        public void TestFuncWithArrays()
        {
            var xTypeVar = new TypeVariable();
            var yTypeVar = new TypeVariable();
            var zTypeVar = new TypeVariable();
            var wTypeVar = new TypeVariable();
            var wArrayVar = new ArrayType(wTypeVar);
            var zArrayVar = new ArrayType(wArrayVar);

            var intVar = IntType.Instance;
            var xFunVar = new FunType(new[] { yTypeVar, zTypeVar }, wArrayVar);

            var clauses = new List<Clause>
            {
                new Clause
                {
                    Alternatives = new List<List<(IHerbrandObject, IHerbrandObject)>>
                    {
                        new List<(IHerbrandObject, IHerbrandObject)> { (xTypeVar, xFunVar) }
                    }
                },
                new Clause
                {
                    Alternatives = new List<List<(IHerbrandObject, IHerbrandObject)>>
                    {
                        new List<(IHerbrandObject, IHerbrandObject)> { (yTypeVar, zArrayVar), (yTypeVar, intVar) },
                        new List<(IHerbrandObject, IHerbrandObject)> { (yTypeVar, zArrayVar), (zTypeVar, intVar) }
                    }
                },
                new Clause
                {
                    Alternatives = new List<List<(IHerbrandObject, IHerbrandObject)>>
                    {
                        new List<(IHerbrandObject, IHerbrandObject)> { (wTypeVar, zTypeVar) }
                    }
                }
            };

            var solver = new Solver(clauses);
            var expectedSolution = new Dictionary<TypeVariable, IHerbrandObject>
            {
                { xTypeVar, xFunVar },
                { yTypeVar, zArrayVar },
                { zTypeVar, intVar },
                { wTypeVar, intVar }
            };
            CheckSolution(solver, expectedSolution);
        }

        private static void CheckException(Solver solver)
        {
            Assert.ThrowsException<TypeCheckerException>(() => solver.Solve());
        }

        private static void CheckSolution(Solver solver, IDictionary<TypeVariable, IHerbrandObject> expectedOutput)
        {
            var solution = solver.Solve();
            Assert.IsTrue(SameDictionaries(expectedOutput, solution.TypeVariableMapping));
        }

        private static bool SameDictionaries(
            IDictionary<TypeVariable, IHerbrandObject> a,
            IDictionary<TypeVariable, IHerbrandObject> b)
        {
            return a.Count == b.Count && a.Keys.All(key => b.ContainsKey(key) && a[key] == b[key]);
        }
    }
}
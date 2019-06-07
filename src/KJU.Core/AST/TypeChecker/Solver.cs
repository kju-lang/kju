namespace KJU.Core.AST.TypeChecker
{
    using System;
    using System.Collections.Generic;
    using AST.Types;

    /// <summary>
    ///   Solves satisfiability problem in propositional calculus with equality.
    /// </summary>
    public class Solver
    {
        public Solver(List<Clause> clauses)
        {
        }

        /// <summary>
        ///   Solve the problem. Throws exception if:
        ///   - there are no solutions
        ///   - there are multiple solutions
        ///   - but does not need to throw if there are non-instantiated type variables
        ///
        ///   Solution may contain TypeVariables on right hand side - the only requirement is that
        ///   all Clauses have to be satisfied.
        /// </summary>
        public Solution Solve()
        {
            throw new NotImplementedException();
        }
    }
}

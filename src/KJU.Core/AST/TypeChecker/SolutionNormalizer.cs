namespace KJU.Core.AST.TypeChecker
{
    using System;
    using System.Collections.Generic;
    using AST.Types;

    public class SolutionNormalizer
    {
        /// Normalize the solution - make sure that there are no TypeVariables on right hand sides.
        /// Throw if there is invalid recursion (e.g. X == [X]). Notably, recursion in function types
        /// is allowed (e.g. X -> Y == X).
        /// Throw if solution contain an uninstantiated type variable.
        public static Solution Normalize(Solution solution)
        {
            throw new NotImplementedException();
        }
    }
}

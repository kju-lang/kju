namespace KJU.Core.AST.TypeChecker
{
    using System;
    using System.Collections.Generic;
    using AST.Types;
    using Lexer;

    /// <summary>
    ///   A clause in our satisfability problem.
    ///
    ///   The clause says that after unification the following should hold (pseudocode):
    ///      this.Alternatives.Any(conj => conj.All(t => t.Item0.Equals(t.Item1)))
    /// </summary>
    public class Clause
    {
        /// Location in the source code that produced this clause.
        public Range InputRange { get; set; }

        public List<List<(IHerbrandObject, IHerbrandObject)>> Alternatives { get; set; }
    }
}

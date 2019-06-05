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
        public Clause()
        {
        }

        public Clause(List<List<(IHerbrandObject, IHerbrandObject)>> alternatives, Range inputRange)
        {
            this.Alternatives = alternatives;
            this.InputRange = inputRange;
        }

        public Clause(List<(IHerbrandObject, IHerbrandObject)> onlyAlternative, Range inputRange)
            : this(new List<List<(IHerbrandObject, IHerbrandObject)>> { onlyAlternative }, inputRange)
        {
        }

        public Clause((IHerbrandObject, IHerbrandObject) equality, Range inputRange)
            : this(new List<(IHerbrandObject, IHerbrandObject)> { equality }, inputRange)
        {
        }

        /// Location in the source code that produced this clause.
        public Range InputRange { get; set; }

        public List<List<(IHerbrandObject, IHerbrandObject)>> Alternatives { get; set; }
    }
}

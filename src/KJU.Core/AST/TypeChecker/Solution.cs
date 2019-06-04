namespace KJU.Core.AST.TypeChecker
{
    using System;
    using System.Collections.Generic;
    using AST.Types;

    public struct Solution
    {
        public IDictionary<TypeVariable, IHerbrandObject> TypeVariableMapping { get; set; }

        public IDictionary<Clause, int> ChosenAlternative { get; set; }
    }
}

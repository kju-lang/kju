namespace KJU.Core.CodeGeneration.CfgLinearizer
{
    using System;
    using System.Collections.Generic;
    using Intermediate;

    public interface ICfgLinearizer
    {
        Tuple<IReadOnlyList<Tree>, IReadOnlyDictionary<ILabel, int>> Linearize(ILabel cfg);
    }
}
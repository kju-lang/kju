namespace KJU.Core.Lexer
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class ConflictResolver<TLabel>
    where TLabel : IComparable
    {
        private TLabel noneValue;

        public ConflictResolver(TLabel noneValue)
        {
            this.noneValue = noneValue;
        }

        public TLabel ResolveWithMaxValue(IEnumerable<TLabel> tokenCategories)
        {
            if (tokenCategories.Count() == 0)
                return this.noneValue;
            else
                return tokenCategories.Max();
        }

        public TLabel ResolveWithMinValue(IEnumerable<TLabel> tokenCategories)
        {
            if (tokenCategories.Count() == 0)
                return this.noneValue;
            else
                return tokenCategories.Min();
        }
    }
}

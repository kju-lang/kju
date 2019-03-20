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
            var tokenCategoriesList = new List<TLabel>(tokenCategories);
            if (tokenCategoriesList.Count == 0)
                return this.noneValue;
            else
                return tokenCategoriesList.Max();
        }

        public TLabel ResolveWithMinValue(IEnumerable<TLabel> tokenCategories)
        {
            var tokenCategoriesList = new List<TLabel>(tokenCategories);
            if (tokenCategoriesList.Count == 0)
                return this.noneValue;
            else
                return tokenCategoriesList.Min();
        }
    }
}

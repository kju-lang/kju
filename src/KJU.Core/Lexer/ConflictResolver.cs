namespace KJU.Core.Lexer
{
    using System;
    using System.Collections.Generic;

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
            TLabel highestValue = this.noneValue;

            foreach (TLabel label in tokenCategories)
            {
                if (label.CompareTo(highestValue) > 0)
                {
                    highestValue = label;
                }
            }

            return highestValue;
        }
    }
}

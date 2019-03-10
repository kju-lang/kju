namespace KJU.Core.Automata
{
    using System.Collections.Generic;
    using System.Linq;

    public class ListState<T> : IState
    {
        public ListState(IList<T> val)
        {
            this.Value = new List<T>(val);
        }

        public IList<T> Value { get; set; }

        public bool Equals(IState iother)
        {
            return (this as object).Equals(iother);
        }

        public override bool Equals(object iother)
        {
            switch (iother)
            {
                case ListState<T> other:
                    return this.Value.SequenceEqual(other.Value);
                default:
                    return false;
            }
        }

        public override int GetHashCode()
        {
            return this.Value.Aggregate(1, (acc, x) => unchecked(acc * 137) ^ (x == null ? 0 : x.GetHashCode()));
        }

        public override string ToString()
        {
            return string.Join(", ", this.Value.Select(x => (x == null) ? "null" : x.ToString()));
        }
    }
}
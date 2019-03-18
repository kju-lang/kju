namespace KJU.Core.Automata
{
    public class ValueState<T> : IState
    {
        public ValueState(T val)
        {
            this.Value = val;
        }

        public T Value { get; }

        public bool Equals(IState iother)
        {
            return (this as object).Equals(iother);
        }

        public override bool Equals(object iother)
        {
            switch (iother)
            {
                case ValueState<T> other:
                    return this.Value.Equals(other.Value);
                default:
                    return false;
            }
        }

        public override int GetHashCode()
        {
            return this.Value.GetHashCode();
        }

        public override string ToString()
        {
            return this.Value.ToString();
        }
    }
}
namespace KJU.Core.Regex
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using KJU.Core.Automata;

    public sealed class AtomicRegex : Regex
    {
        public AtomicRegex(char value)
        {
            this.Value = value;
        }

        public char Value { get; }

        public override bool Equals(object other)
        {
            if (!(other is AtomicRegex))
            {
                return false;
            }

            var otherAtomic = (AtomicRegex)other;
            return this.Value.Equals(otherAtomic.Value);
        }

        public override int GetHashCode()
        {
            return this.Value.GetHashCode();
        }

        public override string ToString()
        {
            return $"AtomicRegex{{{this.Value}}}";
        }
    }
}
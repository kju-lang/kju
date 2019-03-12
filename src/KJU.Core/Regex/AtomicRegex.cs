namespace KJU.Core.Regex
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using KJU.Core.Automata;

    public sealed class AtomicRegex<Symbol> : Regex<Symbol>
    {
        public AtomicRegex(Symbol value)
        {
            this.Value = value;
        }

        public Symbol Value { get; }

        public override bool Equals(object other)
        {
            if (!(other is AtomicRegex<Symbol>))
            {
                return false;
            }

            var otherAtomic = (AtomicRegex<Symbol>)other;
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
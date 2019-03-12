namespace KJU.Core.Regex
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using KJU.Core.Automata;

    public sealed class StarRegex<Symbol> : Regex<Symbol>
    {
        public StarRegex(Regex<Symbol> child)
        {
            this.Child = child;
        }

        public Regex<Symbol> Child { get; }

        public override bool Equals(object other)
        {
            if (!(other is StarRegex<Symbol>))
            {
                return false;
            }

            var otherStar = (StarRegex<Symbol>)other;
            return this.Child.Equals(otherStar.Child);
        }

        public override int GetHashCode()
        {
            return this.Child.GetHashCode();
        }

        public override string ToString()
        {
            return $"StarRegex{{{this.Child}}}";
        }
    }
}
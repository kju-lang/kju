namespace KJU.Core.Regex
{
    using System.Collections.Generic;

    public sealed class SumRegex<Symbol> : Regex<Symbol>
    {
        public SumRegex(Regex<Symbol> left, Regex<Symbol> right)
        {
            this.Left = left;
            this.Right = right;
        }

        public Regex<Symbol> Left { get; }

        public Regex<Symbol> Right { get; }

        public override bool Equals(object other)
        {
            if (!(other is SumRegex<Symbol>))
            {
                return false;
            }

            var sumOther = (SumRegex<Symbol>)other;

            return this.Left.Equals(sumOther.Left) && this.Right.Equals(sumOther.Right);
        }

        public override int GetHashCode()
        {
            return this.Left.GetHashCode() + (this.Right.GetHashCode() * 31);
        }

        public override string ToString()
        {
            return $"SumRegex{{{this.Left}, {this.Right}}}";
        }
    }
}
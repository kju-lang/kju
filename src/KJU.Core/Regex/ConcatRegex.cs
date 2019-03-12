namespace KJU.Core.Regex
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;

    public sealed class ConcatRegex<Symbol> : Regex<Symbol>
    {
        public ConcatRegex(Regex<Symbol> left, Regex<Symbol> right)
        {
            this.Left = left;
            this.Right = right;
        }

        public Regex<Symbol> Left { get; }

        public Regex<Symbol> Right { get; }

        public override bool Equals(object other)
        {
            if (!(other is ConcatRegex<Symbol>))
            {
                return false;
            }

            var concatOther = (ConcatRegex<Symbol>)other;
            return this.Left.Equals(concatOther.Left) && this.Right.Equals(concatOther.Right);
        }

        public override int GetHashCode()
        {
            return this.Left.GetHashCode() + (this.Right.GetHashCode() * 31);
        }

        public override string ToString()
        {
            return $"ConcatRegex{{{this.Left}, {this.Right}}}";
        }
    }
}
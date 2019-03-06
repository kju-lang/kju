namespace KJU.Core.Regex
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;

    public sealed class ConcatRegex : Regex
    {
        public ConcatRegex(Regex left, Regex right)
        {
            this.Left = left;
            this.Right = right;
        }

        public Regex Left { get; }

        public Regex Right { get; }

        public override bool Equals(object other)
        {
            if (!(other is ConcatRegex))
            {
                return false;
            }

            var concatOther = (ConcatRegex)other;
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
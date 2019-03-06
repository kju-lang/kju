namespace KJU.Core.Regex
{
    using System.Collections.Generic;

    public sealed class SumRegex : Regex
    {
        public SumRegex(Regex left, Regex right)
        {
            this.Left = left;
            this.Right = right;
        }

        public Regex Left { get; }

        public Regex Right { get; }

        public override bool Equals(object other)
        {
            if (!(other is SumRegex))
            {
                return false;
            }

            var sumOther = (SumRegex)other;

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
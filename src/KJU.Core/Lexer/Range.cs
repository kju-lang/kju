namespace KJU.Core.Lexer
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using KJU.Core.Input;

    public class Range
    {
        public Range(ILocation begin, ILocation end)
        {
            this.Begin = begin;
            this.End = end;
        }

        public ILocation Begin { get; }

        public ILocation End { get; }

        public override bool Equals(object obj)
        {
            if (obj is Range other)
            {
                return Equals(this.Begin, other.Begin) && Equals(this.End, other.End);
            }

            return false;
        }

        public override int GetHashCode()
        {
            return ((this.Begin != null ? this.Begin.GetHashCode() : 0) * 397) ^
                   (this.End != null ? this.End.GetHashCode() : 0);
        }

        public override string ToString()
        {
            return $"{{{this.Begin}, {this.End}}}";
        }
    }
}
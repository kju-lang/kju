namespace KJU.Core.Input
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    public class StringLocation : ILocation
    {
        public StringLocation(int position)
        {
            this.Position = position;
        }

        public int Position { get; }

        public override bool Equals(object obj)
        {
            if (obj is StringLocation otherStringLocation)
            {
                return this.Position.Equals(otherStringLocation.Position);
            }

            return false;
        }

        public override int GetHashCode()
        {
            return this.Position.GetHashCode();
        }

        public override string ToString()
        {
            return $"{this.Position}";
        }
    }
}
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

        public override string ToString()
        {
            return $"{this.Position}";
        }
    }
}

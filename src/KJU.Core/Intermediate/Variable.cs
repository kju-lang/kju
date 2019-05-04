namespace KJU.Core.Intermediate
{
    using System;

    public class Variable
    {
        public Variable(Function.Function owner, ILocation location)
        {
            this.Owner = owner;
            this.Location = location ?? throw new Exception("Location is null");
        }

        public Function.Function Owner { get; }

        public ILocation Location { get; }
    }
}
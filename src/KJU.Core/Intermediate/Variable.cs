namespace KJU.Core.Intermediate
{
    public class Variable
    {
        public Variable(Function.Function owner, ILocation location)
        {
            this.Owner = owner;
            this.Location = location;
        }

        public Variable()
        {
        }

        public Function.Function Owner { get; set; }

        public ILocation Location { get; set; }
    }
}
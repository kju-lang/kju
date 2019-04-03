namespace KJU.Core.Intermediate
{
    public class Variable
    {
        public Function Owner { get; set; }

        public ILocation Location { get; set; }
    }
}
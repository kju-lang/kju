namespace KJU.Core.Intermediate
{
    public class Computation
    {
        public Computation(ILabel start, Node result)
        {
            this.Start = start;
            this.Result = result;
        }

        public Computation(ILabel start)
            : this(start, new UnitImmediateValue())
        {
        }

        public ILabel Start { get; private set; }

        public Node Result { get; }
    }
}

namespace KJU.Core.Intermediate
{
    public class Computation
    {
        public Computation(Label start, Node result)
        {
            this.Start = start;
            this.Result = result;
        }

        public Computation(Label start)
            : this(start, new UnitImmediateValue())
        {
        }

        public Label Start { get; private set; }

        public Node Result { get; }

        public void ReplaceStartLabel(Label newStart)
        {
            newStart.Tree = this.Start.Tree;
            this.Start = newStart;
        }
    }
}

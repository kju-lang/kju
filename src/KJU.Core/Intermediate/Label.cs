namespace KJU.Core.Intermediate
{
    public class Label
    {
        public Label(Tree tree)
        {
            this.Tree = tree;
        }

        public Tree Tree { get; set; }

        public string Id { get; set; }
    }
}
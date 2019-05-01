namespace KJU.Core.Intermediate
{
    public class Label
    {
        public Label(Tree tree)
        {
            this.Tree = tree;
        }

        public string Id { get; set; }

        public Tree Tree { get; set; }

        public override string ToString()
        {
            return $"Label{{Id: {this.Id ?? "null"}, Tree: {this.Tree}}}";
        }
    }
}
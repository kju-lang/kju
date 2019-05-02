namespace KJU.Core.Intermediate
{
    using System;

    public class Label
    {
        private bool alreadyBuild;
        private Tree tree;

        public Label(Tree tree)
        {
            this.Tree = tree ?? throw new Exception("Tree is null.");
            this.alreadyBuild = true;
        }

        private Label()
        {
            this.alreadyBuild = false;
        }

        public string Id { get; set; }

        public Tree Tree
        {
            get
            {
                if (!this.alreadyBuild)
                {
                    throw new Exception("Access before build.");
                }

                return this.tree;
            }
            set => this.tree = value;
        }

        public static T WithLabel<T>(Func<Label, (Tree, T)> treeGetter)
        {
            var label = new Label();
            var (tree, result) = treeGetter.Invoke(label);
            if (tree == null)
            {
                throw new Exception("Tree is null.");
            }

            label.Tree = tree;
            label.alreadyBuild = true;
            return result;
        }

        public override string ToString()
        {
            return $"Label{{Id: {this.Id ?? "null"}, Tree: {this.Tree}}}";
        }
    }
}
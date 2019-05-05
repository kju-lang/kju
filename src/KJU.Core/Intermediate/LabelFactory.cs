namespace KJU.Core.Intermediate
{
    using System;
    using CodeGeneration.FunctionToAsmGeneration;

    public class LabelFactory : ILabelFactory
    {
        private readonly ILabelIdGenerator labelIdGenerator;

        public LabelFactory(ILabelIdGenerator labelIdGenerator)
        {
            this.labelIdGenerator = labelIdGenerator;
        }

        public ILabel GetLabel(Tree tree)
        {
            return new Label(this.labelIdGenerator.GenerateLabelId(), tree);
        }

        public T WithLabel<T>(Func<ILabel, (Tree, T)> treeGetter)
        {
            var label = new Label(this.labelIdGenerator.GenerateLabelId());
            var (tree, result) = treeGetter.Invoke(label);
            if (tree == null)
            {
                throw new Exception("Tree is null.");
            }

            label.Tree = tree;
            label.AlreadyBuild = true;
            return result;
        }

        private class Label : ILabel
        {
            private Tree tree;

            public Label(string id, Tree tree)
            {
                this.Id = id;
                this.Tree = tree ?? throw new Exception("Tree is null.");
                this.AlreadyBuild = true;
            }

            public Label(string id)
            {
                this.Id = id;
                this.AlreadyBuild = false;
            }

            public bool AlreadyBuild { private get; set; }

            public string Id { get; }

            public Tree Tree
            {
                get
                {
                    if (!this.AlreadyBuild)
                    {
                        throw new Exception("Access before build.");
                    }

                    return this.tree;
                }
                set => this.tree = value;
            }

            public override string ToString()
            {
                return $"Label{{Id: {this.Id},\nTree: {this.Tree}\n}}";
            }
        }
    }
}
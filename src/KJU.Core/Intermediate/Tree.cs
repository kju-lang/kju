namespace KJU.Core.Intermediate
{
    public class Tree
    {
        public Tree(Node root)
        {
            this.Root = root;
        }

        public Node Root { get; }

        public ControlFlowInstruction ControlFlow { get; set; }
    }
}
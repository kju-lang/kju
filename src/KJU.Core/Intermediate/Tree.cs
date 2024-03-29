namespace KJU.Core.Intermediate
{
    public class Tree
    {
        public Tree(Node root, ControlFlowInstruction controlFlow)
        {
            this.Root = root;
            this.ControlFlow = controlFlow;
        }

        public Node Root { get; }

        public ControlFlowInstruction ControlFlow { get; }

        public override string ToString()
        {
            return $"Tree{{Root: {this.Root}, ControlFlowInstruction: {this.ControlFlow}}}";
        }
    }
}
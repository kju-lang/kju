namespace KJU.Core.Intermediate.FunctionBodyGenerator
{
    internal struct LoopLabels
    {
        public LoopLabels(Label condition, Label after)
        {
            this.Condition = condition;
            this.After = after;
        }

        public Label Condition { get; }

        public Label After { get; }
    }
}
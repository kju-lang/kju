namespace KJU.Core.Intermediate.FunctionGeneration
{
    internal struct LoopLabels
    {
        public LoopLabels(ILabel condition, ILabel after)
        {
            this.Condition = condition;
            this.After = after;
        }

        public ILabel Condition { get; }

        public ILabel After { get; }
    }
}
#pragma warning disable SA1402 // File may only contain a single class
namespace KJU.Core.Intermediate
{
    public class ControlFlowInstruction
    {
    }

    public class UnconditionalJump : ControlFlowInstruction
    {
        public UnconditionalJump(Label target)
        {
            this.Target = target;
        }

        public Label Target { get; }
    }

    public class ConditionalJump : ControlFlowInstruction
    {
        public ConditionalJump(Label trueTarget, Label falseTarget)
        {
            this.TrueTarget = trueTarget;
            this.FalseTarget = falseTarget;
        }

        public Label TrueTarget { get; }

        public Label FalseTarget { get; }
    }

    public class FunctionCall : ControlFlowInstruction
    {
        public FunctionCall(Function func, Label targetAfter)
        {
            this.Func = func;
            this.TargetAfter = targetAfter;
        }

        public Function Func { get; }

        public Label TargetAfter { get; }
    }

    public class Ret : ControlFlowInstruction
    {
    }
}
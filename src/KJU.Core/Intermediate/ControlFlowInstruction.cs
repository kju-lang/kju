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
        public Label TrueTarget { get; set; }

        public Label FalseTarget { get; set; }
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
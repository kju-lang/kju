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

        public override string ToString()
        {
            return $"UnconditionalJump{{Target: {this.Target}}}";
        }
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

        public override string ToString()
        {
            return $"ConditionalJump{{TrueTarget: {this.TrueTarget}, FalseTarget: {this.FalseTarget}}}";
        }
    }

    public class FunctionCall : ControlFlowInstruction
    {
        public FunctionCall(Function.Function func, Label targetAfter)
        {
            this.Func = func;
            this.TargetAfter = targetAfter;
        }

        public Function.Function Func { get; }

        public Label TargetAfter { get; }

        public override string ToString()
        {
            return $"FunctionCall{{Function: {this.Func}, TargetAfter: {this.TargetAfter}}}";
        }
    }

    public class Ret : ControlFlowInstruction
    {
        public override string ToString()
        {
            return "Ret";
        }
    }
}
#pragma warning disable SA1402 // File may only contain a single class
namespace KJU.Core.Intermediate
{
    public class ControlFlowInstruction
    {
    }

    public class UnconditionalJump : ControlFlowInstruction
    {
        public UnconditionalJump(ILabel target)
        {
            this.Target = target;
        }

        public ILabel Target { get; }

        public override string ToString()
        {
            return $"UnconditionalJump{{Target: {this.Target}}}";
        }
    }

    public class ConditionalJump : ControlFlowInstruction
    {
        public ConditionalJump(ILabel trueTarget, ILabel falseTarget)
        {
            this.TrueTarget = trueTarget;
            this.FalseTarget = falseTarget;
        }

        public ILabel TrueTarget { get; }

        public ILabel FalseTarget { get; }

        public override string ToString()
        {
            return $"ConditionalJump{{TrueTarget: {this.TrueTarget}, FalseTarget: {this.FalseTarget}}}";
        }
    }

    public class FunctionCall : ControlFlowInstruction
    {
        public FunctionCall(Function.Function function, ILabel targetAfter)
        {
            this.Function = function;
            this.TargetAfter = targetAfter;
        }

        public Function.Function Function { get; }

        public ILabel TargetAfter { get; }

        public override string ToString()
        {
            return $"FunctionCall{{Function: {this.Function}, TargetAfter: {this.TargetAfter}}}";
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
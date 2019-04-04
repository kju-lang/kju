#pragma warning disable SA1402 // File may only contain a single class
namespace KJU.Core.Intermediate
{
    public class ControlFlowInstruction
    {
    }

    public class UnconditionalJump : ControlFlowInstruction
    {
        public Label Target { get; set; }
    }

    public class ConditionalJump : ControlFlowInstruction
    {
        public Label TrueTarget { get; set; }

        public Label FalseTarget { get; set; }
    }

    public class FunctionCall : ControlFlowInstruction
    {
        public Function Func { get; set; }

        public Label TargetAfter { get; set; }
    }

    public class Ret : ControlFlowInstruction
    {
    }
}
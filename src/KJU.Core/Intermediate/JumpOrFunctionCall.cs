#pragma warning disable SA1402 // File may only contain a single class
namespace KJU.Core.Intermediate
{
    public class JumpOrFunctionCall
    {
    }

    public class UnconditionalJump : JumpOrFunctionCall
    {
        public Label Target { get; set; }
    }

    public class ConditionalJump : JumpOrFunctionCall
    {
        public Label TrueTarget { get; set; }

        public Label FalseTarget { get; set; }
    }

    public class FunctionCall : Node
    {
        public Function Func { get; set; }
    }
}
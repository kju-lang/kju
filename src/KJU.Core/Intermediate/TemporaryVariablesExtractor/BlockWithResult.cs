namespace KJU.Core.Intermediate.TemporaryVariablesExtractor
{
    using System.Collections.Generic;
    using System.Linq;
    using AST;
    using KJU.Core.Lexer;

    internal class BlockWithResult : Expression
    {
        // Helper node: execute body, then 'return' result
        public BlockWithResult(Range inputRange, InstructionBlock body, Expression result)
            : base(inputRange)
        {
            this.Body = body;
            this.Result = result;
        }

        public InstructionBlock Body { get; }

        public Expression Result { get; }

        public override IEnumerable<Node> Children()
        {
            return this.Body.Children().Concat(new[] { this.Result as Node });
        }
    }
}
namespace KJU.Core.AST.ParseTreeToAstConverter
{
    internal interface IOperationOrderFlipper
    {
        void AddEnclosedWithParentheses(Node node);

        void FlipToLeftAssignmentAst(Node ast);
    }
}
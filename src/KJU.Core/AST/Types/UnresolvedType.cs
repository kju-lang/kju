namespace KJU.Core.AST.Types
{
    using KJU.Core.Lexer;

    public class UnresolvedType : DataType
    {
        public UnresolvedType(string type, Range inputRange)
        {
            this.Type = type;
            this.InputRange = inputRange;
        }

        public string Type { get; }

        public Range InputRange { get; }

        public override string ToString()
        {
            return $"UnresolvedType: {this.Type} ";
        }
    }
}
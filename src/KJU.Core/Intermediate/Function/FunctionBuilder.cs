namespace KJU.Core.Intermediate.Function
{
    using AST;
    using NameMangler;

    public class FunctionBuilder
    {
        private readonly INameMangler nameMangler;

        public FunctionBuilder(INameMangler nameMangler)
        {
            this.nameMangler = nameMangler;
        }

        public Function CreateFunction(FunctionDeclaration functionDeclaration, Function parentFunction)
        {
            var mangledName = this.nameMangler.GetMangledName(functionDeclaration, parentFunction?.MangledName);
            var parameters = functionDeclaration.Parameters;
            var isEntryPoint = functionDeclaration.IsEntryPoint;
            var isForeign = functionDeclaration.IsForeign;
            return new Function(
                parentFunction,
                mangledName,
                parameters,
                isEntryPoint,
                isForeign);
        }
    }
}
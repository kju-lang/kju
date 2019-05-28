namespace KJU.Core.Intermediate.Function
{
    using System.Linq;
    using AST;
    using NameMangler;
    using static AST.Nodes.NodeUtils;

    public class FunctionBuilder
    {
        public static bool HasChildFunctions(FunctionDeclaration root)
        {
            return root.ChildrenRecursive().OfType<FunctionDeclaration>().Any();
        }

        public static Function CreateFunction(FunctionDeclaration functionDeclaration, Function parentFunction)
        {
            var mangledName = NameMangler.GetMangledName(functionDeclaration, parentFunction?.MangledName);
            var parameters = functionDeclaration.Parameters;
            var isEntryPoint = functionDeclaration.IsEntryPoint;
            var isForeign = functionDeclaration.IsForeign;
            return new Function(
                parentFunction,
                mangledName,
                parameters,
                isEntryPoint,
                isForeign,
                hasChildFunctions: HasChildFunctions(functionDeclaration));
        }
    }
}

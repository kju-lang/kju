namespace KJU.Core.Intermediate.NameMangler
{
    using AST;

    public interface INameMangler
    {
        string GetMangledName(FunctionDeclaration declaration, string parentMangledName);
    }
}
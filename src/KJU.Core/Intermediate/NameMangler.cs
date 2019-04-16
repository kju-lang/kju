namespace KJU.Core.Intermediate
{
    using System;
    using System.Linq;
    using AST;
    using AST.BuiltinTypes;

    public static class NameMangler
    {
        public static string GetMangledName(FunctionDeclaration declaration, string parentMangledName)
        {
            string name = declaration.Identifier;
            string result = $"{name.Count()}{name}";

            if (parentMangledName == null)
                result = "_ZN3KJU" + result + "E"; // KJU namespace
            else
                result = "_Z" + parentMangledName.Substring(1) + "EN" + result + "E";

            if (declaration.Parameters.Count() == 0)
            {
                result += "v";
            }
            else
            {
                foreach (var arg in declaration.Parameters)
                {
                    result += MangleTypeName(arg.VariableType);
                }
            }

            return result;
        }

        public static string MangleTypeName(DataType type)
        {
            switch (type)
            {
                case BoolType t:
                    return "b";
                case IntType t:
                    return "x";
                case UnitType t:
                    return "v";
                default:
                    throw new ArgumentException($"unknown type: {type}");
            }
        }
    }
}
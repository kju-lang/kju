namespace KJU.Core.Intermediate.NameMangler
{
    using System;
    using System.Linq;
    using AST;
    using AST.BuiltinTypes;
    using AST.Types;

    public class NameMangler : INameMangler
    {
        public string GetMangledName(FunctionDeclaration declaration, string parentMangledName)
        {
            string name = declaration.Identifier;
            string result = $"{name.Length}{name}";

            if (parentMangledName == null)
            {
                result = $"_ZN3KJU{result}E"; // KJU namespace
            }
            else
            {
                result = $"_Z{parentMangledName.Substring(1)}EN{result}E";
            }

            if (declaration.Parameters.Any())
            {
                result = declaration.Parameters.Aggregate(result, (current, arg) => current + MangleTypeName(arg.VariableType));
            }
            else
            {
                result += "v";
            }

            return result;
        }

        private static string MangleTypeName(DataType type)
        {
            switch (type)
            {
                case BoolType _:
                    return "b";
                case IntType _:
                    return "x";
                case UnitType _:
                    return "v";
                case ArrayType arrayType:
                    return $"P{MangleTypeName(arrayType.ElementType)}";
                case StructType structType:
                    return $"Ts{structType.Name}";
                default:
                    throw new ArgumentException($"Unknown type: {type}");
            }
        }
    }
}
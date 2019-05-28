namespace KJU.Core.Intermediate.NameMangler
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AST;
    using AST.BuiltinTypes;
    using AST.Types;

    public class NameMangler
    {
        public static string MangleTypeName(DataType type)
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
                    return $"Ts{structType.Name}_{structType.Id}_";
                default:
                    throw new ArgumentException($"Unknown type: {type}");
            }
        }

        public static string GetMangledName(FunctionDeclaration declaration, string parentMangledName)
        {
            return GetMangledName(declaration.Identifier, declaration.Parameters.Select(param => param.VariableType).ToList(), parentMangledName);
        }

        public static string GetMangledName(string name, IReadOnlyList<DataType> paramTypes, string parentMangledName)
        {
            string result = $"{name.Length}{name}";

            if (parentMangledName == null)
            {
                result = $"_ZN3KJU{result}E"; // KJU namespace
            }
            else
            {
                result = $"_Z{parentMangledName.Substring(1)}EN{result}E";
            }

            if (paramTypes.Count > 0)
            {
                result += string.Join(string.Empty, paramTypes.Select(type => MangleTypeName(type)));
            }
            else
            {
                result += "v";
            }

            return result;
        }
    }
}

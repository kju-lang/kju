namespace KJU.Core.AST.Types
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    public class StructType : DataType
    {
        private static Dictionary<StructDeclaration, StructType> instances = new Dictionary<StructDeclaration, StructType>();

        private StructType(StructDeclaration declaration)
        {
            this.Name = declaration.Name;
            this.Declaration = declaration;
        }

        public string Name { get; }

        public StructDeclaration Declaration { get; }

        public static StructType GetInstance(StructDeclaration structDeclaration)
        {
            if (!instances.ContainsKey(structDeclaration))
            {
                instances.Add(structDeclaration, new StructType(structDeclaration));
            }

            return instances[structDeclaration];
        }

        public override string ToString()
        {
            return $"Struct {this.Declaration}";
        }
    }
}

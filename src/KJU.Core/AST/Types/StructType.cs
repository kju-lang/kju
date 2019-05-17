namespace KJU.Core.AST.Types
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    public class StructType : DataType
    {
        public StructType(string name)
        {
            this.Name = name;
        }

        public StructType(StructDeclaration declaration)
        {
            this.Name = declaration.Name;
            this.Declaration = declaration;
        }

        public string Name { get; }

        public StructDeclaration Declaration { get; }

        public override string ToString()
        {
            return $"Struct {this.Declaration}";
        }
    }
}

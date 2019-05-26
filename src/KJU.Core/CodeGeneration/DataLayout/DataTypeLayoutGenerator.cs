namespace KJU.Core.CodeGeneration.DataLayout
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using KJU.Core.AST;
    using KJU.Core.AST.Types;

    public class DataTypeLayoutGenerator
    {
        public IEnumerable<string> GenerateDataLayouts(Node root)
        {
            return this.CollectTypes(root).SelectMany(dataType => dataType.GenerateLayout());
        }

        public HashSet<DataType> CollectTypes(Node root)
        {
            var dataTypes = new HashSet<DataType>();
            this.CollectTypes(root, dataTypes);

            return dataTypes;
        }

        private void CollectTypes(Node node, HashSet<DataType> dataTypes)
        {
            foreach (var child in node.Children())
                this.CollectTypes(child, dataTypes);

            if (node is Expression)
            {
                this.AddTypeWithSubtypes(((Expression)node).Type, dataTypes);

                switch (node)
                {
                    case StructDeclaration structDeclaration:
                        this.AddTypeWithSubtypes(StructType.GetInstance(structDeclaration), dataTypes);
                        break;

                    case VariableDeclaration variableDeclaration:
                        this.AddTypeWithSubtypes(variableDeclaration.VariableType, dataTypes);
                        break;
                }
            }
        }

        private void AddTypeWithSubtypes(DataType type, HashSet<DataType> dataTypes)
        {
            if (dataTypes.Contains(type))
                return;

            switch (type)
            {
                case StructType structType:
                    dataTypes.Add(structType);

                    foreach (var field in structType.Declaration.Fields)
                        this.AddTypeWithSubtypes(field.Type, dataTypes);

                    break;

                case ArrayType arrayType:
                    dataTypes.Add(arrayType);
                    this.AddTypeWithSubtypes(arrayType.ElementType, dataTypes);

                    break;
            }
        }
    }
}
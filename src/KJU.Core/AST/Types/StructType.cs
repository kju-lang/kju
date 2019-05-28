namespace KJU.Core.AST.Types
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    public class StructType : DataType
    {
        public StructType(string name, IReadOnlyList<StructField> fields)
        {
            this.Id = $"{Guid.NewGuid():N}";
            this.Name = name;
            this.Fields = fields;
        }

        public string Id { get; }

        public string Name { get; }

        public IReadOnlyList<StructField> Fields { get; set; }

        public static StructType GetInstance(StructDeclaration structDeclaration)
        {
            if (structDeclaration.StructType == null)
            {
                structDeclaration.StructType = new StructType(structDeclaration.Name, structDeclaration.Fields);
            }

            return structDeclaration.StructType;
        }

        public override string ToString()
        {
            return $"Struct {string.Join(",", this.Fields)}";
        }

        public override IEnumerable<string> GenerateLayout()
        {
            yield return $"{this.LayoutLabel}:";
            yield return "dq 0"; // Not an array type.
            int pos = 0;
            foreach (var field in this.Fields)
            {
                if (!(field.Type is ArrayType) && !(field.Type is StructType))
                {
                    ++pos;
                    continue;
                }

                yield return $"dq {pos}";
                yield return $"dq {field.Type.LayoutLabel}";
                ++pos;
            }

            yield return "dq 0";
            // Null target: end of layout.
            yield return "dq 0";
        }

        public override bool IsHeapType()
        {
            return true;
        }
    }
}

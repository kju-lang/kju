namespace KJU.Core.AST.Types
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    public class StructType : DataType
    {
        private static readonly Dictionary<StructDeclaration, StructType> Instances =
            new Dictionary<StructDeclaration, StructType>();

        private StructType(StructDeclaration declaration)
        {
            this.Id = Instances.Count;
            this.Name = declaration.Name;
            this.Declaration = declaration;
        }

        public int Id { get; }

        public string Name { get; }

        public StructDeclaration Declaration { get; }

        public static StructType GetInstance(StructDeclaration structDeclaration)
        {
            if (!Instances.ContainsKey(structDeclaration))
            {
                Instances.Add(structDeclaration, new StructType(structDeclaration));
            }

            return Instances[structDeclaration];
        }

        public override string ToString()
        {
            return $"Struct {this.Declaration}";
        }

        public override IEnumerable<string> GenerateLayout()
        {
            yield return $"{this.LayoutLabel}:";
            yield return "dq 0"; // Not an array type.
            int pos = 0;
            foreach (var field in this.Declaration.Fields)
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
namespace KJU.Core.AST.Types
{
    using System.Collections.Generic;
    using System.Linq;

    public class ArrayType : DataType
    {
        private static readonly Dictionary<DataType, ArrayType> Instances = new Dictionary<DataType, ArrayType>();

        private ArrayType(DataType elementType)
        {
            this.ElementType = elementType;
        }

        public DataType ElementType { get; }

        public static ArrayType GetInstance(DataType elementType)
        {
            if (!Instances.ContainsKey(elementType))
                Instances.Add(elementType, new ArrayType(elementType));
            return Instances[elementType];
        }

        public override string ToString()
        {
            return $"[{this.ElementType.ToString()}]";
        }

        public override IEnumerable<string> GenerateLayout()
        {
            yield return $"{this.LayoutLabel}:";
            if (!(this.ElementType is ArrayType) && !(this.ElementType is StructType))
                yield return "dq 0, 0, 0"; // Act as a struct type without pointer fields.
            else
                yield return $"dq {this.ElementType.LayoutLabel}";
        }

        public override bool IsHeapType()
        {
            return true;
        }
    }
}
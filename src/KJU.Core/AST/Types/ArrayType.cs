namespace KJU.Core.AST.Types
{
    using System.Collections.Generic;
    using System.Linq;

    public class ArrayType : DataType
    {
        public ArrayType(DataType elementType)
        {
            this.ElementType = elementType;
        }

        public DataType ElementType { get; }

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

        public override bool Equals(object obj)
        {
            return this.Equals(obj as ArrayType);
        }

        public bool Equals(ArrayType other)
        {
            return other != null && this.ElementType.Equals(other.ElementType);
        }

        public override int GetHashCode()
        {
            return ("Array", this.ElementType).GetHashCode();
        }

        public override IEnumerable<IHerbrandObject> GetArguments()
        {
            yield return this.ElementType;
        }
    }
}

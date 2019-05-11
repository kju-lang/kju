namespace KJU.Core.AST.Types
{
    using System.Collections.Generic;

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
    }
}
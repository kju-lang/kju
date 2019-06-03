namespace KJU.Core.AST.Types
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class FunType : DataType, IEquatable<FunType>
    {
        public FunType(IEnumerable<DataType> argTypes, DataType resultType)
        {
            this.ArgTypes = argTypes.ToList();
            this.ResultType = resultType;
        }

        public FunType(FunctionDeclaration declaration)
            : this(declaration.Parameters.Select(param => param.VariableType), declaration.ReturnType)
        {
        }

        public override string LayoutLabel
        {
            get
            {
                return "1";
            }
        }

        public IReadOnlyList<DataType> ArgTypes { get; }

        public DataType ResultType { get; }

        public override bool IsHeapType()
        {
            return true;
        }

        public override bool Equals(object obj)
        {
            return this.Equals(obj as FunType);
        }

        public bool Equals(FunType other)
        {
            return other != null
                && this.ResultType.Equals(other.ResultType)
                && Enumerable.SequenceEqual(this.ArgTypes, other.ArgTypes);
        }

        public override int GetHashCode()
        {
            return this.ArgTypes.Aggregate(
                this.ResultType.GetHashCode(),
                (acc, type) => (acc, type).GetHashCode());
        }
    }
}

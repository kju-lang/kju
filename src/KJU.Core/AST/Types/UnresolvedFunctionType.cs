namespace KJU.Core.AST.Types
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class UnresolvedFunctionType : DataType
    {
        public UnresolvedFunctionType(IEnumerable<DataType> argTypes, DataType resultType)
        {
            this.ArgTypes = argTypes.ToList();
            this.ResultType = resultType;
        }

        public IReadOnlyList<DataType> ArgTypes { get; }

        public DataType ResultType { get; }

        public override bool IsHeapType()
        {
            throw new ArgumentException("unresolved type");
        }
    }
}

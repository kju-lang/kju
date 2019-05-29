namespace KJU.Core.AST.Types
{
    using System;
    using System.Collections.Generic;

    public class FunType : DataType
    {
        public IReadOnlyList<DataType> ArgTypes { get; }

        public DataType ResultType { get; }

        public override bool IsHeapType()
        {
            throw new NotImplementedException();
        }
    }
}
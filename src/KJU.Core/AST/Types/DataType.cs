namespace KJU.Core.AST
{
    using System;
    using System.Collections.Generic;

    public abstract class DataType
    {
        public string LayoutLabel { get; }

        public IEnumerable<string> GenerateLayout()
        {
            throw new NotImplementedException();
        }
    }
}
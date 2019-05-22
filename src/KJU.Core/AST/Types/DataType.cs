namespace KJU.Core.AST
{
    using System;
    using System.Collections.Generic;
    using Intermediate.NameMangler;

    public abstract class DataType
    {
        public string LayoutLabel
        {
            get
            {
                return "L_" + NameMangler.MangleTypeName(this);
            }
        }

        public abstract bool IsHeapType();

        public virtual IEnumerable<string> GenerateLayout()
        {
            throw new NotImplementedException();
        }
    }
}
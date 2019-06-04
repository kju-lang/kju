namespace KJU.Core.AST
{
    using System;
    using System.Collections.Generic;
    using Intermediate.NameMangler;
    using Types;

    public abstract class DataType : IHerbrandObject
    {
        public virtual string LayoutLabel
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

        public virtual object GetTag()
        {
            return this.GetType();
        }

        public virtual IEnumerable<IHerbrandObject> GetArguments()
        {
            return new List<IHerbrandObject>();
        }
    }
}

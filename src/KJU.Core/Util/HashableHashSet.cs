namespace KJU.Core.Util
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Text;

    public class HashableHashSet<T> : HashSet<T>
    {
        public override bool Equals(object other)
        {
            if (!(other is HashableHashSet<T>))
            {
                return false;
            }

            return this.Equals(other as HashableHashSet<T>);
        }

        public bool Equals(HashableHashSet<T> other)
        {
            if (this.Count != other.Count)
            {
                return false;
            }

            foreach (T e in this)
            {
                if (!other.Contains(e))
                {
                    return false;
                }
            }

            return true;
        }

        public override int GetHashCode()
        {
            int h = 0;
            foreach (T e in this)
            {
                h += e.GetHashCode();
            }

            return h;
        }
    }
}

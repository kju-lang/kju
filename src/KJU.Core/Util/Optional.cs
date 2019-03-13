namespace KJU.Core.Util
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Text;

    public struct Optional<T>
        where T : class
    {
        // works only for nullable types
        private T value;

        private Optional(T value)
        {
            this.value = value;
        }

        public static Optional<T> Some(T value)
        {
            if (value == null)
            {
                throw new ArgumentException("Optional.Some didn't expect null");
            }

            return new Optional<T>(value);
        }

        public static Optional<T> None()
        {
            return new Optional<T>(null);
        }

        public bool IsSome()
        {
            return this.value != null;
        }

        public bool IsNone()
        {
            return this.value == null;
        }

        public T Get()
        {
            if (this.IsNone())
            {
                throw new ArgumentException("attempt to retrieve value from None");
            }

            return this.value;
        }

        public T GetOrNull()
        {
            return this.value;
        }

        public override bool Equals(object obj)
        {
            switch (obj)
            {
                case Optional<T> other:
                    return object.Equals(this.value, other.value);
                default:
                    return false;
            }
        }

        public override int GetHashCode()
        {
            if (this.value == null)
            {
                return 0;
            }
            else
            {
                return this.value.GetHashCode();
            }
        }
    }
}
namespace KJU.Core.Regex
{
    using System.Collections.Generic;
    using System.Linq;

    public static class RegexUtils
    {
        public static Regex<T> ToRegex<T>(this T atom)
        {
            return new AtomicRegex<T>(atom);
        }

        public static Regex<T> Sum<T>(params Regex<T>[] children)
        {
            if (children.Length == 0)
            {
                return new EmptyRegex<T>();
            }

            var childrenList = new List<Regex<T>>(children);
            var first = childrenList[0];
            childrenList.RemoveAt(0);
            return childrenList.Aggregate(first, (acc, x) => new SumRegex<T>(acc, x));
        }

        public static Regex<T> Concat<T>(params Regex<T>[] children)
        {
            if (children.Length == 0)
            {
                return new EpsilonRegex<T>();
            }

            var childrenList = new List<Regex<T>>(children);
            var first = childrenList[0];
            childrenList.RemoveAt(0);
            return childrenList.Aggregate(first, (acc, x) => new ConcatRegex<T>(acc, x));
        }

        public static Regex<T> Starred<T>(this Regex<T> child)
        {
            return new StarRegex<T>(child);
        }

        public static Regex<T> Optional<T>(this Regex<T> child)
        {
            return Sum(new EpsilonRegex<T>(), child);
        }

        public static Regex<T> SeparatedBy<T>(this Regex<T> element, Regex<T> separator)
        {
            return Concat(
                element,
                Concat(separator, element).Starred()).Optional();
        }
    }
}
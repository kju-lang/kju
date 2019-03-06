namespace KJU.Core.Regex
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using KJU.Core.Automata;

    public sealed class EpsilonRegex : Regex
    {
        public override bool Equals(object other)
        {
            return other is EpsilonRegex;
        }

        public override int GetHashCode()
        {
            return 0;
        }

        public override string ToString()
        {
            return "EpsilonRegex";
        }
    }
}

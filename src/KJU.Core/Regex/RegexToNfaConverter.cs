namespace KJU.Core.Regex
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using KJU.Core.Automata;

    public static class RegexToNfaConverter
    {
        public static INfa Convert(Regex regex)
        {
            switch (regex)
            {
                case AtomicRegex atomic:
                    break;
                case ConcatRegex concat:
                    break;

                // ...
            }

            throw new NotImplementedException();
        }
    }
}

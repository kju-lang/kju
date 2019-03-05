namespace KJU.Core.Regex
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using KJU.Core.Automata;

    public abstract class Regex
    {
        /*
         * AtomicRegex - regex containing one letter (char)
         * SumRegex - regex being sum of two (A|B)
         * ConcatRegex - binary(!) catenation of two regexes
         * StarRegex - R*
         * EpsilonRegex - regex accepting empty word
         * EmptyRegex - not accepting anything
         *
         * Each implementation will contain additional info e.g. children, letter
         *
         * Regex is a tree structure with leafs being epsilon or letter
         * Implement converting string to Regex class and converting Regex to Nfa
         */
    }
}

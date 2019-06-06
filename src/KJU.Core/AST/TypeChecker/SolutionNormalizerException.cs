namespace KJU.Core.AST.TypeChecker
{
    using System;

    public class SolutionNormalizerException : Exception
    {
        public SolutionNormalizerException(string message)
            : base(message)
        {
        }
    }
}
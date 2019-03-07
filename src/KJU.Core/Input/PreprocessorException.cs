namespace KJU.Core.Input
{
    using System;

    public class PreprocessorException : Exception
    {
        private ILocation location;

        public PreprocessorException()
        {
            this.location = null;
        }

        public PreprocessorException(string message)
            : base(message)
        {
            this.location = null;
        }

        public PreprocessorException(string message, ILocation location = null)
            : base(message)
        {
            this.location = location;
        }
    }
}
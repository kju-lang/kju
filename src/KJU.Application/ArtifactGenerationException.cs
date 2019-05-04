namespace KJU.Application
{
    using System;

    public class ArtifactGenerationException : Exception
    {
        public ArtifactGenerationException(string message)
            : base(message)
        {
        }
    }
}
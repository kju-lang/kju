namespace KJU.Core.CodeGeneration.Templates
{
    using System;

    public class TemplateCreationException : Exception
    {
        public TemplateCreationException(string message)
            : base(message)
        {
        }
    }
}
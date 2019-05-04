namespace KJU.Core.Intermediate
{
    using System;

    public interface ILabel
    {
        string Id { get; }

        Tree Tree { get; set; }
    }
}
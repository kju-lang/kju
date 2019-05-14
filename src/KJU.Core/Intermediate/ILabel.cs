namespace KJU.Core.Intermediate
{
    public interface ILabel
    {
        string Id { get; }

        Tree Tree { get; set; }
    }
}
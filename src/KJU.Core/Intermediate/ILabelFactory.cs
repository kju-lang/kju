namespace KJU.Core.Intermediate
{
    using System;

    public interface ILabelFactory
    {
        ILabel GetLabel(Tree tree);

        T WithLabel<T>(Func<ILabel, (Tree, T)> treeGetter);
    }
}
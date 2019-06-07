namespace KJU.Core.AST.TypeChecker
{
    using System;

    /// <summary>
    /// For each set in the structure we want to be able to return the "representant" of the set.
    /// When we merge to sets the representant of the new set is decided by the arbiter
    /// i.e. representant[union(x, y)] := arbiter(representant[x], representant[y]) or smth similar.
    /// </summary>
    public class FindUnion<T>
        where T : class
    {
        public FindUnion(Func<T, T, T> arbiter)
        {
            throw new NotImplementedException();
        }

        public void PushCheckpoint()
        {
            throw new NotImplementedException();
        }

        public void PopCheckpoint()
        {
            throw new NotImplementedException();
        }

        public void Union(T a, T b)
        {
            throw new NotImplementedException();
        }

        public T GetParent(T x)
        {
            throw new NotImplementedException();
        }

        public T GetRepresentant(T x)
        {
            throw new NotImplementedException();
        }
    }
}

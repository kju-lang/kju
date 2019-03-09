namespace KJU.Core.Automata
{
    using System;

    public interface IState : IEquatable<IState>
    {
        int GetHashCode();

        bool Equals(object o);
    }
}

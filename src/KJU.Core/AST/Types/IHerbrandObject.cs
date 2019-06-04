namespace KJU.Core.AST.Types
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    ///   An object from Herbrand universe.
    ///
    ///   All KJU types implement this interface - it is required by
    ///   the type inference solver for type unification.
    ///
    ///   We say two Herbrand objects are equal when they have the same tag
    ///   and theirs arguments are equal.
    /// </summary>
    public interface IHerbrandObject
    {
        /// Returns a tag that identifies this kind of objects.
        /// (e.g. for FunType it will be "FunType").
        object GetTag();

        IEnumerable<IHerbrandObject> GetArguments();
    }
}

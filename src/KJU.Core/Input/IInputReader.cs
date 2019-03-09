namespace KJU.Core.Input
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    public interface IInputReader
    {
        List<KeyValuePair<ILocation, char>> Read();
    }
}

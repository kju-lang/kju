namespace KJU.Core.Input
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    public class Preprocessor
    {
        public IEnumerable<KeyValuePair<ILocation, char>>
            PreprocessInput(IEnumerable<KeyValuePair<ILocation, char>> input)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<KeyValuePair<ILocation, char>>
            RemoveComments(IEnumerable<KeyValuePair<ILocation, char>> input)
        {
            throw new NotImplementedException();
        }
    }
}

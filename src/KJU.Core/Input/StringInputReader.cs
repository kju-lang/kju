namespace KJU.Core.Input
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    public class StringInputReader : IInputReader
    {
        public StringInputReader(string input)
        {
            this.Input = input;
        }

        public string Input { get; }

        public IEnumerable<KeyValuePair<ILocation, char>> ReadGenerator()
        {
            var result = (this.Input + KJU.Core.Constants.EndOfInput)
                .Select((c, index) => new KeyValuePair<ILocation, char>(new StringLocation(index), c));
            return result;
        }

        public List<KeyValuePair<ILocation, char>> Read()
        {
            return this.ReadGenerator().ToList();
        }
    }
}
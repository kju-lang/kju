namespace KJU.Core.Input
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;

    public class FileInputReader : IInputReader
    {
        public FileInputReader(string fileName)
        {
            this.FileName = fileName;
        }

        public string FileName { get; }

        public List<KeyValuePair<ILocation, char>> Read()
        {
            return this.ReadGenerator().ToList();
        }

        private IEnumerable<KeyValuePair<ILocation, char>> ReadGenerator()
        {
            return File.ReadLines(this.FileName)
                .Select(line => line + "\n")
                .Append(char.ToString(KJU.Core.Constants.EndOfInput))
                .SelectMany((line, lineIndex) =>
                    line.Select((c, columnIndex) =>
                        new KeyValuePair<ILocation, char>(new KJU.Core.Input.FileLocation(this.FileName, lineIndex + 1, columnIndex + 1), c)));
        }
    }
}
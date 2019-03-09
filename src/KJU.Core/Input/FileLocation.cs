namespace KJU.Core.Input
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    public class FileLocation : ILocation
    {
        public FileLocation(string fileName, int line, int column)
        {
            this.FileName = fileName;
            this.Line = line;
            this.Column = column;
        }

        public string FileName { get; }

        public int Line { get; }

        public int Column { get; }

        public override string ToString()
        {
            return $"{this.FileName}:{this.Line}:{this.Column}";
        }
    }
}

namespace KJU.Tests.Util
{
    using System;

    public static class KjuHome
    {
        public static readonly string Path = Environment.GetEnvironmentVariable("KJU_HOME") ?? System.IO.Path.GetFullPath("../../../../..");
    }
}

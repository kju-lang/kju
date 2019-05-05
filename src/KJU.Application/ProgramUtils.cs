namespace KJU.Application
{
    using System;

    public static class ProgramUtils
    {
        public static int RunProcess(string program, string arguments)
        {
            Console.WriteLine($"Running: {program} {arguments}");
            var process = new System.Diagnostics.Process
            {
                StartInfo =
                {
                    FileName = program,
                    Arguments = arguments,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden,
                    CreateNoWindow = true
                }
            };
            process.Start();
            process.WaitForExit();
            return process.ExitCode;
        }
    }
}
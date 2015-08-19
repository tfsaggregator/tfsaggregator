namespace Aggregator.ConsoleApp
{
    using System;
    using System.IO;

    internal static class ExceptionExtensions
    {
        public static void Dump(this Exception e, TextWriter console)
        {
            console.Write("Error: ");

            Exception toLog = e;
            while (toLog != null)
            {
                console.WriteLine(e.Message);
                toLog = toLog.InnerException;
            }
        }
    }
}

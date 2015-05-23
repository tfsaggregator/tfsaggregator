namespace Aggregator.ConsoleApp
{
    using System;
    using System.IO;

    static class ExceptionExtensions
    {
        public static void Dump(this Exception e, TextWriter console)
        {
            console.Write("Error: ");
            while (e != null)
            {
                console.WriteLine(e.Message);
                e = e.InnerException;
            }//while
        }
    }
}

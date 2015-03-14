using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aggregator.ConsoleApp
{
    static class ExceptionExtensions
    {
        public static void Dump(this Exception e, System.IO.TextWriter console)
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

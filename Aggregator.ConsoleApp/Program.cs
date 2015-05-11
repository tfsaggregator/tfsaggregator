using ManyConsole;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aggregator.ConsoleApp
{
    class Program
    {
        static int Main(string[] args)
        {
            Console.Write(GetHeader());

            try
            {
                // locate any commands in the assembly (or use an IoC container, or whatever source)
                var commands = ConsoleCommandDispatcher.FindCommandsInSameAssemblyAs(typeof(Program));
                // then run them.
                int rc = ConsoleCommandDispatcher.DispatchCommand(commands, args, Console.Out);
                if (rc == 0)
                {
                    ConsoleColor save = Console.ForegroundColor;
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("Succeeded.");
                    Console.ForegroundColor = save;
                }//if
                return rc;
            }
            catch (Exception e)
            {
                e.Dump(Console.Out);
                return 99;
            }//try
        }

        static private T GetCustomAttribute<T>()
            where T : Attribute
        {
            return System.Reflection.Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(T), false).FirstOrDefault() as T;
        }

        static internal string GetHeader()
        {
            var title = GetCustomAttribute<System.Reflection.AssemblyTitleAttribute>();
            var descr = GetCustomAttribute<System.Reflection.AssemblyDescriptionAttribute>();
            var copy = GetCustomAttribute<System.Reflection.AssemblyCopyrightAttribute>();
            var config = GetCustomAttribute<System.Reflection.AssemblyConfigurationAttribute>();
            var fileVersion = GetCustomAttribute<System.Reflection.AssemblyFileVersionAttribute>();
            var infoVersion = GetCustomAttribute<System.Reflection.AssemblyInformationalVersionAttribute>();

            var sb = new StringBuilder();
            sb.AppendFormat("{0} {1}", title.Title, infoVersion.InformationalVersion);
            sb.AppendLine();
            sb.AppendLine(descr.Description);
            sb.AppendFormat("Build: {0}, Configuration: {1}", fileVersion.Version, config.Configuration);
            sb.AppendLine();
            sb.AppendLine(copy.Copyright);

            return sb.ToString();
        }
    }
}

namespace Aggregator.ConsoleApp
{
    using System;
    using System.Linq;
    using System.Reflection;
    using System.Text;

    using ManyConsole;

    internal static class Program
    {
        internal static int Main(string[] args)
        {
            Console.Write(GetHeader());

            int rc = 42; // generic failure
            try
            {
                // locate any commands in the assembly (or use an IoC container, or whatever source)
                var commands = ConsoleCommandDispatcher.FindCommandsInSameAssemblyAs(typeof(Program));

                // then run them.
                rc = ConsoleCommandDispatcher.DispatchCommand(commands, args, Console.Out);
                if (rc == 0)
                {
                    ConsoleColor save = Console.ForegroundColor;
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("Succeeded.");
                    Console.ForegroundColor = save;
                }
            }
            catch (Exception e)
            {
                e.Dump(Console.Out);
                rc = 99;
            }

            return rc;
        }

        internal static string GetHeader()
        {
            var title = GetCustomAttribute<AssemblyTitleAttribute>();
            var descr = GetCustomAttribute<AssemblyDescriptionAttribute>();
            var copy = GetCustomAttribute<AssemblyCopyrightAttribute>();
            var config = GetCustomAttribute<AssemblyConfigurationAttribute>();
            var fileVersion = GetCustomAttribute<AssemblyFileVersionAttribute>();
            var infoVersion = GetCustomAttribute<AssemblyInformationalVersionAttribute>();

            var sb = new StringBuilder();
            sb.AppendFormat("{0} {1}", title.Title, infoVersion.InformationalVersion);
            sb.AppendLine();
            sb.AppendLine(descr.Description);
            sb.AppendFormat("Build: {0}, Configuration: {1}", fileVersion.Version, config.Configuration);
            sb.AppendLine();
            sb.AppendLine(copy.Copyright);

            return sb.ToString();
        }

        private static T GetCustomAttribute<T>()
            where T : Attribute
        {
            return Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(T), false).FirstOrDefault() as T;
        }
    }
}

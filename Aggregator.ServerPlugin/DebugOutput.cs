#define DEBUG
using System.Diagnostics;

namespace Aggregator.ServerPlugin
{
    internal static class DebugOutput
    {
        public static void WriteLine(string msg)
        {
            Debug.WriteLine(msg);
        }
    }
}

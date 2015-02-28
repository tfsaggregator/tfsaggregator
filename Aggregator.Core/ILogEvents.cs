using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aggregator.Core
{
    public enum LogLevel
    {
        Critical = 1,
        Error = 2,
        Warning = 3,
        Information = 5,
        Normal = Information,
        Verbose = 10,
        Diagnostic = 99,
    }

    public interface ILogEvents
    {
        void WorkItemWrapperTryOpenException(IWorkItem workItem, Exception e);
        void ResultsFromScriptRun(string scriptName, System.Collections.ObjectModel.Collection<System.Management.Automation.PSObject> results);
    }
}

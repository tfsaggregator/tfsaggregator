using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aggregator.Core
{
    using System.Collections.ObjectModel;
    using System.Management.Automation;

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
        void ResultsFromScriptRun(string scriptName, Collection<PSObject> results);
        void ResultsFromScriptRun(string scriptName, object result);
        void ScriptHasError(string scriptName, int line, int column, string errorCode, string errorText);
    }
}

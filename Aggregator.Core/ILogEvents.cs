using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aggregator.Core
{
    using System.Collections.ObjectModel;
    using System.Management.Automation;
    using System.Xml.Schema;


    /// <summary>
    /// Levels of logging.
    /// </summary>
    /// <remarks>While this enumerator is not used within Core, it is read by the configuration class <see cref="TFSAggregatorSettings"/>.</remarks>
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

    /// <summary>
    /// Core Clients will be called on this interface to log events and errors.
    /// </summary>
    /// <remarks>The method *must* not raise exception.</remarks>
    public interface ILogEvents
    {
        void ConfigurationLoaded(string policyFile);
        void StartingProcessing(IRequestContext context, INotification notification);
        void ProcessingCompleted(ProcessingResult result);
        void WorkItemWrapperTryOpenException(IWorkItem workItem, Exception e);
        void ResultsFromScriptRun(string scriptName, Collection<PSObject> results);
        void ResultsFromScriptRun(string scriptName, object result);
        void ScriptHasError(string scriptName, int line, int column, string errorCode, string errorText);
        void ScriptHasWarning(string scriptName, int line, int column, string errorCode, string errorText);
        void Saving(IWorkItem workItem, bool isValid);
        void InvalidConfiguration(XmlSeverityType severity, string message, int lineNumber, int linePosition);
        void UnreferencedRule(string ruleName);
        void ApplyingPolicy(string name);
        void ApplyingRule(string name);
        void BuildingScriptEngine(string scriptLanguage);
        void RunningRule(string name, IWorkItem workItem);

        void FailureLoadingScript(string scriptName);
    }
}

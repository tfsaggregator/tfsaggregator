using Aggregator.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Schema;

namespace Aggregator.ServerPlugin
{
    class ServerEventLogger : LoggerBase, ILogEvents
    {
        public ServerEventLogger(LogLevel level)
            : base(level)
        { }

        public void ResultsFromScriptRun(string scriptName, System.Collections.ObjectModel.Collection<System.Management.Automation.PSObject> results)
        {
            Log(LogLevel.Verbose, "--- Output for script {0} follows", scriptName);
            foreach (var item in results)
            {
                Log(LogLevel.Normal, item.ToString());
            }
            Log(LogLevel.Verbose, "--- Output complete.");
        }

        public void WorkItemWrapperTryOpenException(IWorkItem workItem, Exception e)
        {
            Log(LogLevel.Error, "Unable to open work item '{0}'\nException: {1}", workItem.Id.ToString(), e.Message);
        }

        public void ProcessEventException(Microsoft.TeamFoundation.Framework.Server.TeamFoundationRequestContext requestContext, Exception e)
        {
            if (e.InnerException != null)
            {
                Log(LogLevel.Critical,
                    "Exception encountered processing notification: {0} \nStack Trace:{1}\n    Inner Exception: {2} \nStack Trace:{3}"
                    , e.Message, e.StackTrace
                    , e.InnerException.Message, e.InnerException.StackTrace);
            }
            else
            {
                Log(LogLevel.Critical,
                    "Exception encountered processing notification: {0} \nStack Trace:{1}", e.Message, e.StackTrace);
            }
        }


        public void ResultsFromScriptRun(string scriptName, object result)
        {
            Log(LogLevel.Verbose, "Output from script {0}: {1}", scriptName, result);
        }


        public void ScriptHasError(string scriptName, int line, int column, string errorCode, string errorText)
        {
            Log(LogLevel.Error,
                "Error {3} in script {0} at line {1}, column {2}: {4}",
                scriptName, line, column, errorCode, errorText);
        }

        public void ScriptHasWarning(string scriptName, int line, int column, string errorCode, string errorText)
        {
            Log(LogLevel.Warning,
                "Warning {3} in script {0} at line {1}, column {2}: {4}",
                scriptName, line, column, errorCode, errorText);
        }

        public void Saving(IWorkItem workItem, bool isValid)
        {
            Log(LogLevel.Verbose, "{0} [{1}] {2} valid to save.",
                workItem.TypeName,
                workItem.Id,
                isValid ? "is" : "is NOT");
            if (!isValid)
                Log(LogLevel.Verbose, "Invalid fields: {0}",
                    workItem.GetInvalidWorkItemFieldsList());
        }

        public void InvalidConfiguration(XmlSeverityType severity, string message, int lineNumber, int linePosition)
        {
            switch (severity)
            {
                case XmlSeverityType.Error:
                    Log(LogLevel.Error, "Error in policy file at line {1}, position {2}: {0}", message, lineNumber, linePosition);
                    break;
                case XmlSeverityType.Warning:
                    Log(LogLevel.Warning, "Policy file at line {1}, position {2}: {0}", message, lineNumber, linePosition);
                    break;
                default:
                    Log(LogLevel.Information, "Policy file at line {1}, position {2}: {0}", message, lineNumber, linePosition);
                    break;
            }//switch
        }

        public void UnreferencedRule(string ruleName)
        {
            Log(LogLevel.Warning, "Rule {0} is never used", ruleName);
        }

        public void ConfigurationLoaded(string policyFile)
        {
            Log(LogLevel.Information, "Configuration loaded successfully from {0}", policyFile);
        }

        public void StartingProcessing(IRequestContext context, INotification notification)
        {
            Log(LogLevel.Information, "Starting processing on workitem #{0}", notification.WorkItemId);
        }

        public void ProcessingCompleted(ProcessingResult result)
        {
            Log(LogLevel.Information, "Processing completed: {0}", result.StatusMessage);
        }

        public void ApplyingPolicy(string name)
        {
            Log(LogLevel.Verbose, "Applying Policy {0}", name);
        }

        public void ApplyingRule(string name)
        {
            Log(LogLevel.Verbose, "Applying Rule {0}", name);
        }

        public void BuildingScriptEngine(string scriptLanguage)
        {
            Log(LogLevel.Verbose, "Building Script Engine for {0}", scriptLanguage);
        }

        public void RunningRule(string name, IWorkItem workItem)
        {
            Log(LogLevel.Verbose, "Executing Rule {0} on #{1}", name, workItem.Id);
        }

        public void FailureLoadingScript(string scriptName)
        {
            Log(LogLevel.Error, "Failure in parsing {0} script", scriptName);
        }
    }
}

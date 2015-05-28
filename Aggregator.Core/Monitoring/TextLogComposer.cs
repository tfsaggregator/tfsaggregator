namespace Aggregator.Core.Monitoring
{
    using System;
    using System.Collections.ObjectModel;
    using System.Management.Automation;
    using System.Runtime.Caching;
    using System.Xml.Schema;
    using Microsoft.TeamFoundation.Framework.Server;

    public class TextLogComposer : ILogEvents
    {
        ITextLogger logger;
        public TextLogComposer(ITextLogger logger)
        {
            this.logger = logger;
        }

        public ITextLogger TextLogger { get { return logger; } }

        // forward to implementation
        public LogLevel Level
        {
            get
            {
                return logger.Level;
            }

            set
            {
                logger.Level = value;
            }
        }

        public void ResultsFromScriptRun(string scriptName, Collection<PSObject> results)
        {
            logger.Log(LogLevel.Verbose, "--- Output for script '{0}' follows", scriptName);
            foreach (var item in results)
            {
                logger.Log(LogLevel.Normal, item.ToString());
            }
            logger.Log(LogLevel.Verbose, "--- Output complete.");
        }

        public void WorkItemWrapperTryOpenException(IWorkItem workItem, Exception e)
        {
            logger.Log(LogLevel.Error, "Unable to open work item '{0}'\nException: {1}", workItem.Id.ToString(), e.Message);
        }

        public void ProcessEventException(TeamFoundationRequestContext requestContext, Exception e)
        {
            if (e.InnerException != null)
            {
                logger.Log(LogLevel.Critical,
                    "Exception encountered processing notification: {0} \nStack Trace:{1}\n    Inner Exception: {2} \nStack Trace:{3}"
                    , e.Message, e.StackTrace
                    , e.InnerException.Message, e.InnerException.StackTrace);
            }
            else
            {
                logger.Log(LogLevel.Critical,
                    "Exception encountered processing notification: {0} \nStack Trace:{1}", e.Message, e.StackTrace);
            }
        }


        public void ResultsFromScriptRun(string scriptName, object result)
        {
            logger.Log(LogLevel.Verbose, "Output from script '{0}': [{1}]", scriptName, result);
        }


        public void ScriptHasError(string scriptName, int line, int column, string errorCode, string errorText)
        {
            logger.Log(LogLevel.Error,
                "Error {3} in script '{0}' at line {1}, column {2}: {4}",
                scriptName, line, column, errorCode, errorText);
        }

        public void ScriptHasWarning(string scriptName, int line, int column, string errorCode, string errorText)
        {
            logger.Log(LogLevel.Warning,
                "Warning {3} in script '{0}' at line {1}, column {2}: {4}",
                scriptName, line, column, errorCode, errorText);
        }

        public void Saving(IWorkItem workItem, bool isValid)
        {
            logger.Log(LogLevel.Verbose, "{0} [{1}] {2} valid to save.",
                workItem.TypeName,
                workItem.Id,
                isValid ? "is" : "is NOT");
            if (!isValid)
                logger.Log(LogLevel.Verbose, "Invalid fields: {0}",
                    workItem.GetInvalidWorkItemFieldsList());
        }

        public void InvalidConfiguration(XmlSeverityType severity, string message, int lineNumber, int linePosition)
        {
            switch (severity)
            {
                case XmlSeverityType.Error:
                    logger.Log(LogLevel.Error, "Error in policy file at line {1}, position {2}: {0}", message, lineNumber, linePosition);
                    break;
                case XmlSeverityType.Warning:
                    logger.Log(LogLevel.Warning, "Policy file at line {1}, position {2}: {0}", message, lineNumber, linePosition);
                    break;
                default:
                    logger.Log(LogLevel.Information, "Policy file at line {1}, position {2}: {0}", message, lineNumber, linePosition);
                    break;
            }//switch
        }

        public void UnreferencedRule(string ruleName)
        {
            logger.Log(LogLevel.Warning, "Rule '{0}' is never used", ruleName);
        }

        public void ConfigurationLoaded(string policyFile)
        {
            logger.Log(LogLevel.Information, "Configuration loaded successfully from '{0}'", policyFile);
        }

        public void StartingProcessing(IRequestContext context, INotification notification)
        {
            logger.Log(LogLevel.Information, "Starting processing on workitem #{0}", notification.WorkItemId);
        }

        public void ProcessingCompleted(ProcessingResult result)
        {
            logger.Log(LogLevel.Information, "Processing completed: {0}", result.StatusMessage);
        }

        public void ApplyingPolicy(string name)
        {
            logger.Log(LogLevel.Verbose, "Policy '{0}' applies", name);
        }

        public void ApplyingRule(string name)
        {
            logger.Log(LogLevel.Verbose, "Evaluating Rule '{0}'", name);
        }

        public void BuildingScriptEngine(string scriptLanguage)
        {
            logger.Log(LogLevel.Verbose, "Building Script Engine for {0}", scriptLanguage);
        }

        public void RunningRule(string name, IWorkItem workItem)
        {
            logger.Log(LogLevel.Verbose, "Applying Rule '{0}' on #{1}", name, workItem.Id);
        }

        public void FailureLoadingScript(string scriptName)
        {
            logger.Log(LogLevel.Error, "Failure in parsing '{0}' script", scriptName);
        }

        public void AttemptingToMoveWorkItemToState(IWorkItem workItem, string orginalSourceState, string destState)
        {
            logger.Log(LogLevel.Verbose, "Attempting to move {0} [{1}] from '{2}' to state '{3}'"
                , workItem.Type.Name, workItem.Id, orginalSourceState, destState);
        }

        public void WorkItemIsValidToSave(IWorkItem workItem)
        {
            logger.Log(LogLevel.Verbose, "WorkItem {0} [{1}] is valid to save", workItem.Type.Name, workItem.Id);
        }

        public void WorkItemIsInvalidInState(IWorkItem workItem, string destState)
        {
            logger.Log(LogLevel.Warning, "WorkItem is invalid in '{0}' state. Invalid fields: {1}"
                , destState, workItem.GetInvalidWorkItemFieldsList());
        }

        public void LoadingConfiguration(string settingsPath)
        {
            logger.Log(LogLevel.Diagnostic, "Loading Configuration from '{0}' ", settingsPath);
        }

        public void ConfigurationChanged(string settingsPath, CacheEntryRemovedReason removedReason)
        {
            logger.Log(LogLevel.Information, "Configuration file '{0}' changed {1}", settingsPath, removedReason);
        }

        public void UsingCachedConfiguration(string settingsPath)
        {
            logger.Log(LogLevel.Diagnostic, "Using cached Configuration for '{0}' ", settingsPath);
        }
    }
}

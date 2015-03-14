using Aggregator.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aggregator.ConsoleApp
{
    class ConsoleEventLogger : LoggerBase, ILogEvents
    {
        public ConsoleEventLogger(LogLevel level)
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
    }
}

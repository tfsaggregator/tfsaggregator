using Aggregator.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    }
}

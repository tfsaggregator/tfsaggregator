using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Text;

namespace Aggregator.Core
{
    /// <summary>
    /// Invokes Powershell scripting engine
    /// </summary>
    public class ScriptEngine
    {
        ILogEvents logger;
        private string scriptName;
        private string script;
        private IWorkItemRepository store;

        public ScriptEngine(string scriptName, string script, IWorkItemRepository store, ILogEvents logger)
        {
            this.logger = logger;
            this.scriptName = scriptName;
            this.script = script;
            this.store = store;
        }

        public void Run(IWorkItem workItem)
        {
            var config = RunspaceConfiguration.Create();
            using (var runspace = RunspaceFactory.CreateRunspace(config))
            {
                runspace.Open();

                runspace.SessionStateProxy.SetVariable("id", workItem.Id);
                runspace.SessionStateProxy.SetVariable("self", workItem);
                runspace.SessionStateProxy.SetVariable("selfFields", workItem.Fields);
                runspace.SessionStateProxy.SetVariable("selfZ", workItem.Fields["z"]); // <-- works!
                runspace.SessionStateProxy.SetVariable("parent", workItem.Parent);

                using (var ri = new RunspaceInvoke(runspace))
                {
                    // execute
                    var results = ri.Invoke(script);

                    logger.ResultsFromScriptRun(this.scriptName, results);

                    // TODO manage results
                }//using
            }//using
        }
    }
}

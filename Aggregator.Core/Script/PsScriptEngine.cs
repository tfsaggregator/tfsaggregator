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
    public class PsScriptEngine : ScriptEngine
    {
        public PsScriptEngine(string scriptName, string script, IWorkItemRepository store, ILogEvents logger)
            : base(scriptName, script, store, logger)
        {
        }

        override public void Run(IWorkItem workItem)
        {
            var config = RunspaceConfiguration.Create();
            using (var runspace = RunspaceFactory.CreateRunspace(config))
            {
                runspace.Open();

                runspace.SessionStateProxy.SetVariable("id", workItem.Id);
                runspace.SessionStateProxy.SetVariable("self", workItem);
                runspace.SessionStateProxy.SetVariable("selfFields", workItem.Fields);
                runspace.SessionStateProxy.SetVariable("parent", workItem.Parent);

                Pipeline pipeline = runspace.CreatePipeline();
                pipeline.Commands.AddScript(script);

                // execute
                var results = pipeline.Invoke();

                logger.ResultsFromScriptRun(this.scriptName, results);


            }//using
        }
    }
}

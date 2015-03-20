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
        public PsScriptEngine(IWorkItemRepository store, ILogEvents logger)
            : base(store, logger)
        {
        }

        Dictionary<string, string> scripts = new Dictionary<string, string>();

        public override bool Load(string scriptName, string script)
        {
            scripts.Add(scriptName, script);
            return true;
        }

        public override bool LoadCompleted()
        {
            //no-op
            return true;
        }

        override public void Run(string scriptName, IWorkItem workItem)
        {
            string script = scripts[scriptName];

            var config = RunspaceConfiguration.Create();
            using (var runspace = RunspaceFactory.CreateRunspace(config))
            {
                runspace.Open();

                runspace.SessionStateProxy.SetVariable("self", workItem);

                Pipeline pipeline = runspace.CreatePipeline();
                pipeline.Commands.AddScript(script);

                // execute
                var results = pipeline.Invoke();

                logger.ResultsFromScriptRun(scriptName, results);


            }//using
        }
    }
}

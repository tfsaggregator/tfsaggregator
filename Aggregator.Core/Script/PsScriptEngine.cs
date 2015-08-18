using System.Collections.Generic;
using System.Management.Automation.Runspaces;

using Aggregator.Core.Interfaces;
using Aggregator.Core.Monitoring;

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

        private readonly Dictionary<string, string> scripts = new Dictionary<string, string>();

        public override bool Load(string scriptName, string script)
        {
            scripts.Add(scriptName, script);
            return true;
        }

        public override bool LoadCompleted()
        {
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
                runspace.SessionStateProxy.SetVariable("store", this.store);

                Pipeline pipeline = runspace.CreatePipeline();
                pipeline.Commands.AddScript(script);

                // execute
                var results = pipeline.Invoke();

                this.logger.ResultsFromScriptRun(scriptName, results);
            }
        }
    }
}

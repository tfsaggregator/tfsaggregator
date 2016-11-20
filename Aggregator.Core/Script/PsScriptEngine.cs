using System;
using System.Collections.Generic;
using System.Management.Automation.Runspaces;

using Aggregator.Core.Interfaces;
using Aggregator.Core.Monitoring;
using Aggregator.Core.Script;

namespace Aggregator.Core
{
    /// <summary>
    /// Invokes Powershell scripting engine
    /// </summary>
    public class PsScriptEngine : ScriptEngine
    {
        private readonly Dictionary<string, string> scripts = new Dictionary<string, string>();

        public PsScriptEngine(ILogEvents logger, bool debug, IScriptLibrary library)
            : base(logger, debug, library)
        {
        }

        public override void Load(IEnumerable<ScriptSourceElement> sourceElements)
        {
            foreach (var sourceElement in sourceElements)
            {
                // TODO log something
                if (sourceElement.Type != Script.ScriptSourceElementType.Rule)
                {
                    continue;
                }

                this.scripts.Add(sourceElement.Name, sourceElement.SourceCode);
            }
        }

        public override void Run(string scriptName, IWorkItem workItem, IWorkItemRepository store)
        {
            string script = this.scripts[scriptName];

            var config = RunspaceConfiguration.Create();
            using (var runspace = RunspaceFactory.CreateRunspace(config))
            {
                runspace.Open();

                runspace.SessionStateProxy.SetVariable("self", workItem);
                runspace.SessionStateProxy.SetVariable("store", store);
                runspace.SessionStateProxy.SetVariable("logger", this.Logger.ScriptLogger);
                runspace.SessionStateProxy.SetVariable("Library", this.Library);

                Pipeline pipeline = runspace.CreatePipeline();
                pipeline.Commands.AddScript(script);

                // execute
                var results = pipeline.Invoke();

                this.Logger.ResultsFromScriptRun(scriptName, results);
            }
        }
    }
}

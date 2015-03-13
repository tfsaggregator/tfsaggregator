using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aggregator.Core
{
    public abstract class ScriptEngine
    {
        protected ILogEvents logger;
        protected string scriptName;
        protected string script;
        protected IWorkItemRepository store;

        public ScriptEngine(string scriptName, string script, IWorkItemRepository store, ILogEvents logger)
        {
            this.logger = logger;
            this.scriptName = scriptName;
            this.script = script;
            this.store = store;
        }

        public abstract void Run(IWorkItem workItem);
    }
}

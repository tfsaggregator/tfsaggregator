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
        protected IWorkItemRepository store;

        public ScriptEngine(IWorkItemRepository store, ILogEvents logger)
        {
            this.logger = logger;
            this.store = store;
        }

        /// <summary>
        /// Loads the specified script name.
        /// </summary>
        /// <param name="scriptName">Name of the script.</param>
        /// <param name="script">The script.</param>
        /// <returns>true if succeeded</returns>
        public abstract bool Load(string scriptName, string script);
        public abstract bool LoadCompleted();

        public abstract void Run(string scriptName, IWorkItem workItem);
    }
}

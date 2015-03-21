using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aggregator.Core
{
    /// <summary>
    /// Base class for scripting language engines.
    /// </summary>
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
        /// Register the specified <paramref name="script"/> under <paramref name="name"/>.
        /// </summary>
        /// <param name="scriptName">Name of the script.</param>
        /// <param name="script">The script source code.</param>
        /// <returns>true if succeeded</returns>
        /// <remarks>An engine may pre-process/compile the script at this time to get better performances.</remarks>
        public abstract bool Load(string scriptName, string script);
        /// <summary>
        /// Informs the engine that all script has been loaded.
        /// </summary>
        /// <returns></returns>
        public abstract bool LoadCompleted();

        /// <summary>
        /// Runs the  script specified by <paramref name="scriptName">.
        /// </summary>
        /// <param name="scriptName">Name of the script.</param>
        /// <param name="workItem">The work item that must be processed by the script.</param>
        public abstract void Run(string scriptName, IWorkItem workItem);
    }
}

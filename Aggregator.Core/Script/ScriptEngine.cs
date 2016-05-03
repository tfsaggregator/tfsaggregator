using Aggregator.Core.Interfaces;
using Aggregator.Core.Monitoring;

namespace Aggregator.Core
{
    using System;
    using Context;
    using System.Collections.Generic;
    /// <summary>
    /// Base class for scripting language engines.
    /// </summary>
    public abstract class ScriptEngine
    {
        protected ILogEvents Logger { get; }

        protected bool Debug { get; }

        protected ScriptEngine(ILogEvents logger, bool debug)
        {
            this.Logger = logger;
            this.Debug = debug;
        }

        /// <summary>
        /// Runs the  script specified by <paramref name="scriptName" />.
        /// </summary>
        /// <param name="scriptName">Name of the script.</param>
        /// <param name="workItem">The work item that must be processed by the script.</param>
        public abstract void Run(string scriptName, IWorkItem workItem, IWorkItemRepository store);

        internal static ScriptEngine MakeEngine(string scriptLanguage, ILogEvents logger, bool debug)
        {
            logger.BuildingScriptEngine(scriptLanguage);
            Type t = GetScriptEngineType(scriptLanguage);
            var ctor = t.GetConstructor(new Type[] { typeof(ILogEvents), typeof(bool) });
            ScriptEngine engine = ctor.Invoke(new object[] { logger, debug }) as ScriptEngine;
            return engine;
        }

        public abstract void Load(IEnumerable<Script.ScriptSourceElement> sourceElements);

        private static Type GetScriptEngineType(string scriptLanguage)
        {
            // switch cases must match language attribute defined in AggregatorConfiguration.xsd
            switch (scriptLanguage.ToUpperInvariant())
            {
                case "VB":
                case "VB.NET":
                case "VBNET":
                    return typeof(VBNetScriptEngine);

                case "PS":
                case "POWERSHELL":
                    return typeof(PsScriptEngine);

                default:
                case "CS":
                case "CSHARP":
                case "C#":
                    return typeof(CSharpScriptEngine);
            }
        }
    }
}

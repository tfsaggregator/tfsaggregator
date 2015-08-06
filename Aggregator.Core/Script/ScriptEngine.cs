namespace Aggregator.Core
{
    using System;

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
        /// <returns>true when succeeded</returns>
        public abstract bool LoadCompleted();

        /// <summary>
        /// Runs the  script specified by <paramref name="scriptName">.
        /// </summary>
        /// <param name="scriptName">Name of the script.</param>
        /// <param name="workItem">The work item that must be processed by the script.</param>
        public abstract void Run(string scriptName, IWorkItem workItem);

        internal static ScriptEngine MakeEngine(string scriptLanguage, IWorkItemRepository workItemRepository, ILogEvents logger)
        {
            logger.BuildingScriptEngine(scriptLanguage);
            Type t = GetScriptEngineType(scriptLanguage);
            var ctor = t.GetConstructor(new Type[] { typeof(IWorkItemRepository), typeof(ILogEvents) });
            ScriptEngine engine = ctor.Invoke(new object[] { workItemRepository, logger }) as ScriptEngine;
            return engine;
        }

        private static Type GetScriptEngineType(string scriptLanguage)
        {
            switch (scriptLanguage.ToUpperInvariant())
            {
                case "CS":
                case "CSHARP":
                case "C#":
                    return typeof(CSharpScriptEngine);
                case "VB":
                case "VB.NET":
                case "VBNET":
                    return typeof(VBNetScriptEngine);
                case "PS":
                case "POWERSHELL":
                    return typeof(PsScriptEngine);
                default:
                    // TODO Log unsupported or wrong code
                    return typeof(CSharpScriptEngine);
            }
        }
    }
}

using Aggregator.Core.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Aggregator.Core.Facade;
using System.Reflection;
using System.IO;
using System.Runtime.Caching;

namespace Aggregator.Core
{
    /// <summary>
    /// Manages the global inter-call status
    /// </summary>
    public class RuntimeContext : IRuntimeContext
    {
        const string CacheKey = "runtime";
        static MemoryCache cache = new MemoryCache("TFSAggregator2");

        protected RuntimeContext()
        {
            // default
            this.HasErrors = true;
        }

        /// <summary>
        /// Return a proper context
        /// </summary>
        /// <returns></returns>
        public static RuntimeContext GetContext(Func<string> settingsPathGetter, IRequestContext requestContext, ILogEvents logger)
        {
            var runtime = (RuntimeContext)cache.Get(CacheKey);
            if (runtime == null)
            {
                string settingsPath = settingsPathGetter();
                var settings = TFSAggregatorSettings.LoadFromFile(settingsPath, logger);
                runtime = MakeRuntimeContext(settingsPath, settings, requestContext, logger);

                var itemPolicy = new CacheItemPolicy();
                itemPolicy.Priority = CacheItemPriority.NotRemovable;
                itemPolicy.ChangeMonitors.Add(new HostFileChangeMonitor(new List<string>() { settingsPath }));

                cache.Set(CacheKey, runtime, itemPolicy);
            }//if

            return runtime.Clone() as RuntimeContext;
        }

        public static RuntimeContext MakeRuntimeContext(string settingsPath, TFSAggregatorSettings settings, IRequestContext requestContext, ILogEvents logger)
        {
            var runtime = new RuntimeContext();

            runtime.Logger = logger;
            runtime.RequestContext = requestContext;
            runtime.SettingsPath = settingsPath;
            runtime.Settings = settings;
            logger.Level = runtime.Settings.LogLevel;

            runtime.HasErrors = false;
            return runtime;
        }

        public bool HasErrors { get; private set; }
        private List<string> errorList = new List<string>();
        public IEnumerator<string> Errors { get { return errorList.GetEnumerator(); } }

        public IRequestContext RequestContext { get; private set; }
        public string SettingsPath { get; private set; }
        public TFSAggregatorSettings Settings { get; private set; }
        public string Hash { get { return this.Settings.Hash; } }
        public ILogEvents Logger { get; private set; }

        private Dictionary<IWorkItemRepository, ScriptEngine> scriptEngines = new Dictionary<IWorkItemRepository, ScriptEngine>();
        public ScriptEngine GetEngine(IWorkItemRepository workItemStore)
        {
            Func<ScriptEngine> builder = () => {
                var newEngine = ScriptEngine.MakeEngine(this.Settings.ScriptLanguage, workItemStore, this.Logger);
                foreach (var rule in this.Settings.Rules)
                {
                    newEngine.Load(rule.Name, rule.Script);
                }
                newEngine.LoadCompleted();
                return newEngine;
            };

            ScriptEngine engine = null;
            if (scriptEngines.ContainsKey(workItemStore))
            {
                engine = scriptEngines[workItemStore];
            }
            else
            {
                engine = builder();
                scriptEngines.Add(workItemStore, engine);
            }
            return engine;
        }

        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }
}

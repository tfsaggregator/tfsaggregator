using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;

using Aggregator.Core.Configuration;
using Aggregator.Core.Interfaces;
using Aggregator.Core.Monitoring;

namespace Aggregator.Core.Context
{
    /// <summary>
    /// Manages the global inter-call status
    /// </summary>
    public class RuntimeContext : IRuntimeContext
    {
        private const string CacheKey = "runtime";
        private static readonly MemoryCache Cache = new MemoryCache("TFSAggregator2");

        /// <summary>
        /// Initializes a new instance of the <see cref="RuntimeContext"/> class.
        /// </summary>
        protected RuntimeContext()
        {
            // default
            this.HasErrors = true;
        }

        /// <summary>
        /// Return a proper context
        /// </summary>
        /// <returns></returns>
        public static RuntimeContext GetContext(
            Func<string> settingsPathGetter,
            IRequestContext requestContext,
            ILogEvents logger,
            Func<Uri, Microsoft.TeamFoundation.Framework.Client.IdentityDescriptor, ILogEvents, IWorkItemRepository> repoBuilder)
        {
            var runtime = (RuntimeContext)Cache.Get(CacheKey);
            if (runtime == null)
            {
                string settingsPath = settingsPathGetter();
                var settings = TFSAggregatorSettings.LoadFromFile(settingsPath, logger);
                runtime = MakeRuntimeContext(settingsPath, settings, requestContext, logger, repoBuilder);

                var itemPolicy = new CacheItemPolicy();
                itemPolicy.Priority = CacheItemPriority.NotRemovable;
                itemPolicy.ChangeMonitors.Add(new HostFileChangeMonitor(new List<string>() { settingsPath }));

                Cache.Set(CacheKey, runtime, itemPolicy);
            }
            else
            {
                // as it changes at each invocation, must be set again here
                runtime.RequestContext = requestContext;
            }

            return runtime.Clone() as RuntimeContext;
        }

        public static RuntimeContext MakeRuntimeContext(
            string settingsPath,
            TFSAggregatorSettings settings,
            IRequestContext requestContext,
            ILogEvents logger,
            Func<Uri, Microsoft.TeamFoundation.Framework.Client.IdentityDescriptor, ILogEvents, IWorkItemRepository> repoBuilder)
        {
            var runtime = new RuntimeContext();

            runtime.Logger = logger;
            runtime.RequestContext = requestContext;
            runtime.SettingsPath = settingsPath;
            runtime.Settings = settings;
            runtime.RateLimiter = new RateLimiter(runtime);
            logger.MinimumLogLevel = runtime.Settings.LogLevel;
            runtime.repoBuilder = repoBuilder;

            runtime.HasErrors = false;
            return runtime;
        }

        public bool HasErrors { get; private set; }

        private readonly List<string> errorList = new List<string>();

        public IEnumerator<string> Errors
        {
            get
            {
                return this.errorList.GetEnumerator();
            }
        }

        public RateLimiter RateLimiter { get; private set; }

        public IRequestContext RequestContext { get; private set; }

        public string SettingsPath { get; private set; }

        public TFSAggregatorSettings Settings { get; private set; }

        public string Hash
        {
            get
            {
                // TODO instead of GetHashCode need a unique per-Collection Id
                int dictHash = this.scriptEngines.Keys.Aggregate(
                    0,
                    (running, current) => { return (int)(((long)running + current.GetHashCode()) % 0xffffffff); });

                return this.Settings.Hash + dictHash.ToString("x8");
            }
        }

        public ILogEvents Logger { get; private set; }

        private readonly ConcurrentDictionary<IWorkItemRepository, ScriptEngine> scriptEngines = new ConcurrentDictionary<IWorkItemRepository, ScriptEngine>();

        public ScriptEngine GetEngine()
        {
            IWorkItemRepository workItemStore = this.GetWorkItemRepository();

            Func<IWorkItemRepository, ScriptEngine> builder = (store) =>
            {
                var newEngine = ScriptEngine.MakeEngine(this.Settings.ScriptLanguage, workItemStore, this.Logger, this.Settings.Debug);
                foreach (var rule in this.Settings.Rules)
                {
                    newEngine.Load(rule.Name, rule.Script);
                }

                newEngine.LoadCompleted();
                return newEngine;
            };

            ScriptEngine engine = this.scriptEngines.GetOrAdd(workItemStore, builder);
            return engine;
        }

        // isolate type constructor to facilitate Unit testing
        private Func<Uri, Microsoft.TeamFoundation.Framework.Client.IdentityDescriptor, ILogEvents, IWorkItemRepository> repoBuilder;

        public IWorkItemRepository GetWorkItemRepository()
        {
            var collectionUri = this.RequestContext.GetProjectCollectionUri();

            Microsoft.TeamFoundation.Framework.Client.IdentityDescriptor toImpersonate = null;
            if (this.Settings.AutoImpersonate)
            {
                toImpersonate = this.RequestContext.GetIdentityToImpersonate();
            }
                var newRepo = this.repoBuilder(uri, toImpersonate, this.Logger);
                this.Logger.WorkItemRepositoryBuilt(uri, toImpersonate);
                return newRepo;

        }

        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }
}

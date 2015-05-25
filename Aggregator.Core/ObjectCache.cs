namespace Aggregator.Core
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Aggregator.Core.Configuration;
    using System.IO;

    /// <summary>
    /// Base class to enable mocking and unit testing
    /// </summary>
    public abstract class ObjectCacheBase
    {
        protected ILogEvents logger;

        public ObjectCacheBase(ILogEvents logger)
        {
            this.logger = logger;
        }

        public ILogEvents Logger { get { return this.logger; } }

        public abstract TFSAggregatorSettings GetSettings(string settingsPath);
        public abstract ScriptEngine GetEngine(IWorkItemRepository workItemStore, Func<IWorkItemRepository, ScriptEngine> builder);
    }

    /// <summary>
    /// Ad-hoc cache
    /// </summary>
    public class ObjectCache : ObjectCacheBase
    {
        DateTime lastCacheRefresh = DateTime.MinValue;
        TFSAggregatorSettings cachedSettings = null;
        ScriptEngine cachedEngine = null;

        public ObjectCache(ILogEvents logger)
            : base(logger)
        {
        }

        public override TFSAggregatorSettings GetSettings(string settingsPath)
        {
            var updatedOn = File.GetLastWriteTimeUtc(settingsPath);
            if (updatedOn > lastCacheRefresh)
            {
                logger.ConfigurationChanged(settingsPath, lastCacheRefresh, updatedOn);
                var settings = TFSAggregatorSettings.LoadFromFile(settingsPath, this.logger);
                lock (this)
                {
                    lastCacheRefresh = updatedOn;
                    cachedSettings = settings;
                    // invalidates ... whatever
                    cachedEngine = null;
                }
            }
            else
            {
                logger.UsingCachedConfiguration(settingsPath, lastCacheRefresh, updatedOn);
            }
            return cachedSettings;
        }

        public override ScriptEngine GetEngine(IWorkItemRepository workItemStore, Func<IWorkItemRepository, ScriptEngine> builder)
        {
            if (cachedEngine == null)
            {
                var engine = builder(workItemStore);
                lock (this)
                {
                    cachedEngine = engine;
                }
            }

            return cachedEngine;
        }
    }
}

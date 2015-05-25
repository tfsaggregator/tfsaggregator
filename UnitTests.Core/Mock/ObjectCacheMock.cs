using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitTests.Core.Mock
{
    using Aggregator.Core;
    using Aggregator.Core.Configuration;

    class ObjectCacheMock : ObjectCacheBase
    {
        TFSAggregatorSettings settings;

        public ObjectCacheMock(ILogEvents logger, TFSAggregatorSettings settings)
            : base(logger)
        {
            this.settings = settings;
        }

        public override ScriptEngine GetEngine(IWorkItemRepository workItemStore, Func<IWorkItemRepository, ScriptEngine> builder)
        {
            return builder(workItemStore);
        }

        public override TFSAggregatorSettings GetSettings(string settingsPath)
        {
            return this.settings;
        }
    }
}

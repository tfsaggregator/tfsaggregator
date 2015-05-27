using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aggregator.Core
{
    using Aggregator.Core.Configuration;

    public interface IRuntimeContext : ICloneable
    {
        bool HasErrors { get; }
        IEnumerator<string> Errors { get; }

        IRequestContext RequestContext { get; }

        string SettingsPath { get; }
        TFSAggregatorSettings Settings { get; }
        ILogEvents Logger { get; }

        ScriptEngine GetEngine(IWorkItemRepository workItemStore);
    }
}

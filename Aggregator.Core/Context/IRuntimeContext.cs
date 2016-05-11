using System;
using System.Collections.Generic;

using Aggregator.Core.Configuration;
using Aggregator.Core.Interfaces;
using Aggregator.Core.Monitoring;

namespace Aggregator.Core.Context
{
    public interface IRuntimeContext : ICloneable
    {
        bool HasErrors { get; }

        IEnumerator<string> Errors { get; }

        IRequestContext RequestContext { get; }

        string SettingsPath { get; }

        TFSAggregatorSettings Settings { get; }

        ILogEvents Logger { get; }

        IWorkItemRepository WorkItemRepository { get; }

        ScriptEngine GetEngine();

        RateLimiter RateLimiter { get; }

        ConnectionInfo GetConnectionInfo();
    }
}

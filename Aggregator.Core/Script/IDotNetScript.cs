using Aggregator.Core.Interfaces;
using Aggregator.Core.Monitoring;

namespace Aggregator.Core.Script
{
    public interface IDotNetScript
    {
        object RunScript(IWorkItemExposed self, IWorkItemRepositoryExposed store, IRuleLogger scriptLogger, IScriptLibrary library);
    }
}
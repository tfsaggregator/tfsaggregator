using System.Collections;

using Microsoft.TeamFoundation.WorkItemTracking.Client;

namespace Aggregator.Core.Interfaces
{
    /// <summary>
    /// This interface extends <see cref="IWorkItemExposed"/> with any WorkItem feature used in TFS Aggregator
    /// but not exposed to scripts.
    /// </summary>
    public interface IWorkItem : IWorkItemExposed, IWorkItemImplementation
    {
        bool IsDirty { get; }

        void PartialOpen();

        void Save();

        void TryOpen();

        ArrayList Validate();

        IWorkItemType Type { get; }

        bool ShouldLimit(RateLimiter limiter);
    }
}

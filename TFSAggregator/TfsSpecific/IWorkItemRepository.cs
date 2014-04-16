using System;
using System.Collections.Generic;
using TFSAggregator.TfsFacade;

namespace TFSAggregator.TfsSpecific
{
    public interface IWorkItemRepository
    {
        IWorkItem GetWorkItem(int workItemId);
    }
}

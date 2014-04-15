using System;
using TFSAggregator.TfsFacade;
using TFSAggregator.TfSFacade;
namespace TFSAggregator.TfsSpecific
{
    public interface IWorkItemRepository
    {
        IWorkItem GetWorkItem(int workItemId);
    }
}

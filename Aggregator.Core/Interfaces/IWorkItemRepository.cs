using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aggregator.Core
{
    public interface IWorkItemRepository
    {
        IWorkItem GetWorkItem(int workItemId);
        ReadOnlyCollection<IWorkItem> LoadedWorkItems { get; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Aggregator.Core
{
    public interface IWorkItemLink
    {
        string LinkTypeEndImmutableName { get; }
        int TargetId { get; }
        IWorkItem Target { get; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.TeamFoundation.WorkItemTracking.Client;

namespace Aggregator.Core.Interfaces
{
    public interface IFieldExposed : IField
    {
        Field TfsField { get; }
    }
}

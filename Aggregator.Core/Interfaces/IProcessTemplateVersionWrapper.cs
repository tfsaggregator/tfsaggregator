using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aggregator.Core.Interfaces
{
    public interface IProcessTemplateVersionWrapper
    {
        Guid TypeId { get; set; }

        int Major { get; set; }

        int Minor { get; set; }
    }
}

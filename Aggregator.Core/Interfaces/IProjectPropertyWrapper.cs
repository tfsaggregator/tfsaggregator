using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aggregator.Core.Interfaces
{
    public interface IProjectPropertyWrapper
    {
        string Name { get; set; }
        string Value { get; set; }
    }
}

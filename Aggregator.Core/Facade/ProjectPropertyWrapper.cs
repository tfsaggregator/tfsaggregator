using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aggregator.Core.Facade
{
    using Aggregator.Core.Interfaces;

    public class ProjectPropertyWrapper : IProjectPropertyWrapper
    {
        public string Name { get; set; }

        public string Value { get; set; }
    }
}

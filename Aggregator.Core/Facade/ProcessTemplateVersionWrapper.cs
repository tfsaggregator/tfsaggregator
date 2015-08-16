using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aggregator.Core.Facade
{
    using Aggregator.Core.Interfaces;

    public class ProcessTemplateVersionWrapper : IProcessTemplateVersionWrapper
    {
        public ProcessTemplateVersionWrapper()
        {
        }

        public ProcessTemplateVersionWrapper(Guid typeId, int major, int minor)
        {
            this.TypeId = typeId;
            this.Major = major;
            this.Minor = minor;
        }

        public Guid TypeId { get; set; }

        public int Major { get; set; }

        public int Minor { get; set; }
    }
}

using Aggregator.Core;
using Microsoft.TeamFoundation.Framework.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aggregator.Core.Facade
{
    public class RequestContextWrapper : IRequestContext
    {
        private TeamFoundationRequestContext context;
        public RequestContextWrapper(TeamFoundationRequestContext context)
        {
            this.context = context;
        }

        public string CollectionName
        {
            get
            {
                // HACK
                return context.ServiceHost.VirtualDirectory.Replace("/tfs/", "").Replace("/", "");
            }
        }
    }
}

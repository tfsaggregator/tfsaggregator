using Microsoft.TeamFoundation.Framework.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TFSAggregator.TfsFacade
{
    public class RequestContextWrapper : IRequestContext
    {
        private TeamFoundationRequestContext context;
        public RequestContextWrapper(TeamFoundationRequestContext context)
        {
            this.context = context;
        }
    }
}

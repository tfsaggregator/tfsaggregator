using Microsoft.TeamFoundation.Framework.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TFSAggregator.TfSFacade
{
    public class RequestContext
    {
        private TeamFoundationRequestContext context;
        public RequestContext(TeamFoundationRequestContext context)
        {
            this.context = context;
        }
    }
}

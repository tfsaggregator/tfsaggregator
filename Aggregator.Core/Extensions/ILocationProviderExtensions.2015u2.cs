#if TFS2015u2
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.TeamFoundation.Framework.Server;
using Aggregator.Core.Extensions;
using Microsoft.VisualStudio.Services.Location.Server;
using Microsoft.TeamFoundation.Server.Core.Location;

namespace Aggregator.Core.Extensions
{
    public static class ILocationServiceExtensions
    {
        public static Uri GetSelfReferenceUri(this Microsoft.VisualStudio.Services.Location.Server.ILocationService self, IVssRequestContext context, Microsoft.VisualStudio.Services.Location.AccessMapping mapping)
        {
            string url = self.GetSelfReferenceUrl(context, self.GetDefaultAccessMapping(context));
            return new Uri(url, UriKind.Absolute);
        }
    }
}
#endif
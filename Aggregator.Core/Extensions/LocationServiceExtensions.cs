#if TFS2015u2
using System;

using Microsoft.TeamFoundation.Framework.Server;
using Microsoft.VisualStudio.Services.Location.Server;

namespace Aggregator.Core.Extensions
{
    public static class LocationServiceExtensions
    {
        public static Uri GetSelfReferenceUri(this ILocationService self, IVssRequestContext context, Microsoft.VisualStudio.Services.Location.AccessMapping mapping)
        {
            string url = self.GetSelfReferenceUrl(context, self.GetDefaultAccessMapping(context));
            return new Uri(url, UriKind.Absolute);
        }
    }
}
#endif
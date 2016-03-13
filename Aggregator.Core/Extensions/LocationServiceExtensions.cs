#if TFS2015u2
using System;
using System.Diagnostics.CodeAnalysis;

using Microsoft.TeamFoundation.Framework.Server;
using Microsoft.VisualStudio.Services.Location;
using Microsoft.VisualStudio.Services.Location.Server;

namespace Aggregator.Core.Extensions
{
    public static class LocationServiceExtensions
    {
        [SuppressMessage("Maintainability", "S1172:Unused method parameters should be removed", Justification = "Required by original interface", Scope = "member", Target = "~M:Aggregator.Core.Extensions.LocationServiceExtensions.GetSelfReferenceUri(Microsoft.VisualStudio.Services.Location.Server.ILocationService,Microsoft.TeamFoundation.Framework.Server.IVssRequestContext,Microsoft.VisualStudio.Services.Location.AccessMapping)~System.Uri")]
        public static Uri GetSelfReferenceUri(this ILocationService self, IVssRequestContext context, AccessMapping mapping)
        {
            string url = self.GetSelfReferenceUrl(context, self.GetDefaultAccessMapping(context));
            return new Uri(url, UriKind.Absolute);
        }
    }
}
#endif
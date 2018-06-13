using System;

using Aggregator.Core.Context;

namespace Aggregator.Core.Extensions
{
    public static class UriExtensions
    {
        public static Uri ApplyServerSetting(this Uri requestUri, RuntimeContext context)
        {
            if (context.Settings.ServerBaseUrl == null)
            {
                return requestUri;
            }
            else
            {
                var builder = new UriBuilder(context.Settings.ServerBaseUrl);
                builder.Path = requestUri.AbsolutePath;
                builder.Query = requestUri.Query;
                var uri = builder.Uri;
                return uri;
            }
        }
    }
}

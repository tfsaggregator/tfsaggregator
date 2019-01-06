namespace Aggregator.WebHooks
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Linq;
    using System.Web.Http;
    using Aggregator.Core.Monitoring;
    using Aggregator.WebHooks.Utils;
    using BasicAuthentication.Filters;

    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Web API configuration and services

            // apply the filter to all Web API controllers
            var logger = new AspNetEventLogger("pre-request-parsing", GetDefaultLoggingLevel());
            config.Filters.Add(new IdentityBasicAuthenticationAttribute(logger));

            // Web API routes
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );
        }

        private static LogLevel GetDefaultLoggingLevel()
        {
            var defaultLoggingLevelAsString = ConfigurationManager.AppSettings["DefaultLoggingLevel"];
#pragma warning disable S1854 // Dead stores should be removed
            var defaultLoggingLevel = LogLevel.Normal;
#pragma warning restore S1854 // Dead stores should be removed
            Enum.TryParse<LogLevel>(defaultLoggingLevelAsString, true, out defaultLoggingLevel);
            return defaultLoggingLevel;
        }
    }
}

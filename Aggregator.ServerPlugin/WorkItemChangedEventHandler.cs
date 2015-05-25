namespace TFSAggregator.TfsSpecific
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Reflection;

    using Aggregator.Core;
    using Aggregator.Core.Configuration;
    using Aggregator.Core.Facade;
    using Aggregator.ServerPlugin;

    using Microsoft.TeamFoundation.Client;
    using Microsoft.TeamFoundation.Common;
    using Microsoft.TeamFoundation.Framework.Client;
    using Microsoft.TeamFoundation.Framework.Common;
    using Microsoft.TeamFoundation.Framework.Server;
    using Microsoft.TeamFoundation.WorkItemTracking.Server;

    /// <summary>
    /// The class that subscribes to server side events on the TFS server.
    /// We're only interested in WorkItemChanged events, so we'll filter that out before calling our main logic.
    /// </summary>
    public class WorkItemChangedEventHandler : ISubscriber
    {
        public WorkItemChangedEventHandler()
        {
            //DON"T ADD ANYTHING HERE UNLESS YOU REALLY KNOW WHAT YOU ARE DOING.
            //TFS DOES NOT LIKE CONSTRUCTORS HERE AND SEEMS TO FREEZE WHEN YOU TRY :(
        }

        public Type[] SubscribedTypes()
        {
            return new Type[1] { typeof(WorkItemChangedEvent) };
        }

        ServerEventLogger logger = new ServerEventLogger(LogLevel.Information);

        /// <summary>
        /// This is the one where all the magic starts.  Main() so to speak.  I will load the settings, connect to TFS and apply the aggregation rules.
        /// </summary>
        public EventNotificationStatus ProcessEvent(
            TeamFoundationRequestContext requestContext,
            NotificationType notificationType,
            object notificationEventArgs,
            out int statusCode,
            out string statusMessage,
            out ExceptionPropertyCollection properties)
        {
            string settingsPath = this.GetSettingsFullPath();
            var settings = GetSettingsFromCache(settingsPath, logger);
            if (settings == null)
            {
                statusCode = 99;
                statusMessage = "Errors in configuration file " + settingsPath;
                properties = null;
                return EventNotificationStatus.ActionPermitted;
            }
            logger.Level = settings.LogLevel;
            var uri = this.GetCollectionUriFromContext(requestContext);

            var result = new ProcessingResult();
            try
            {
                //Check if we have a workitem changed event before proceeding
                if (notificationType == NotificationType.Notification && notificationEventArgs is WorkItemChangedEvent)
                {
                    IdentityDescriptor toImpersonate = null;
                    if (settings.AutoImpersonate)
                    {
                        toImpersonate = this.GetIdentityToImpersonate(requestContext, notificationEventArgs as WorkItemChangedEvent);
                    }

                    EventProcessor eventProcessor = new EventProcessor(uri.AbsoluteUri, toImpersonate, logger, settings); //we only need one for the whole app

                    var context = new RequestContextWrapper(requestContext);
                    var notification = new NotificationWrapper(notificationType, notificationEventArgs as WorkItemChangedEvent);

                    logger.StartingProcessing(context, notification);
                    result = eventProcessor.ProcessEvent(context, notification);
                    logger.ProcessingCompleted(result);
                }//if
            }
            catch (Exception e)
            {
                logger.ProcessEventException(requestContext, e);
                // notify failure
                result.StatusCode = -1;
                result.StatusMessage = "Unexpected error: " + e.Message;
                result.NotificationStatus = EventNotificationStatus.ActionPermitted;
            }//try

            statusCode = result.StatusCode;
            statusMessage = result.StatusMessage;
            properties = result.ExceptionProperties;
            return result.NotificationStatus;
        }

        private string GetSettingsFullPath()
        {
            var thisAssembly = Assembly.GetExecutingAssembly();

            // Load the options from file with same name as DLL
            string baseName = thisAssembly.GetName().Name;
            string extension = ".policies";

            // Load the file from same folder where DLL is located
            return Path.Combine(
                        Path.GetDirectoryName(new Uri(thisAssembly.CodeBase).LocalPath),
                        baseName)
                    + extension;
        }

        DateTime lastCacheRefresh = DateTime.MinValue;
        TFSAggregatorSettings cachedSettings = null;

        private TFSAggregatorSettings GetSettingsFromCache(string settingsPath, ILogEvents logger)
        {
            var updatedOn = File.GetLastWriteTimeUtc(settingsPath);
            if (updatedOn > lastCacheRefresh)
            {
                logger.ConfigurationChanged(settingsPath, lastCacheRefresh, updatedOn);
                var settings = TFSAggregatorSettings.LoadFromFile(settingsPath, logger);
                lock (this)
                {
                    lastCacheRefresh = updatedOn;
                    cachedSettings = settings;
                }
            }
            else
            {
                logger.UsingCachedConfiguration(settingsPath, lastCacheRefresh, updatedOn);
            }
            return cachedSettings;
        }

        private Uri GetCollectionUriFromContext(TeamFoundationRequestContext requestContext)
        {
            TeamFoundationLocationService service = requestContext.GetService<TeamFoundationLocationService>();
            return service.GetSelfReferenceUri(requestContext, service.GetDefaultAccessMapping(requestContext));
        }


        private IdentityDescriptor GetIdentityToImpersonate(TeamFoundationRequestContext requestContext, WorkItemChangedEvent workItemChangedEvent)
        {
            Uri server = this.GetCollectionUriFromContext(requestContext);

            var configurationServer = TfsTeamProjectCollectionFactory.GetTeamProjectCollection(server);

            //TODO: Find a way to read the identity from the server object model instead.
            IIdentityManagementService identityManagementService =
            configurationServer.GetService<IIdentityManagementService>();

            TeamFoundationIdentity identity =
                identityManagementService.ReadIdentities(
                    new Guid[] { new Guid(workItemChangedEvent.ChangerTeamFoundationId) },
                    MembershipQuery.None).FirstOrDefault();

            return identity == null ? null : identity.Descriptor;
        }

        public string Name
        {
            get { return "TFSAggregator2"; }
        }

        public SubscriberPriority Priority
        {
            get { return SubscriberPriority.Normal; }
        }
    }
}

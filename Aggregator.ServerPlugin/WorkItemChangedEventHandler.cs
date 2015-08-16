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
    using Microsoft.VisualStudio.Services.ClientNotification;

    using NotificationType = Microsoft.TeamFoundation.Framework.Server.NotificationType;

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
            var runtime = RuntimeContext.GetContext(
                () => GetServerSettingsFullPath(),
                new RequestContextWrapper(requestContext),
                new ServerEventLogger(LogLevel.Normal));
            if (runtime.HasErrors)
            {
                statusCode = 99;
                statusMessage = string.Join(". ", runtime.Errors);
                properties = null;
                return EventNotificationStatus.ActionPermitted;
            }//if

            var logger = runtime.Logger as ServerEventLogger; // HACK remove cast for ProcessEventException

            var result = new ProcessingResult();
            try
            {
                //Check if we have a workitem changed event before proceeding
                if (notificationType == NotificationType.Notification && notificationEventArgs is WorkItemChangedEvent)
                {
                    var uri = this.GetCollectionUriFromContext(requestContext);

                    IdentityDescriptor toImpersonate = null;
                    if (runtime.Settings.AutoImpersonate)
                    {
                        toImpersonate = this.GetIdentityToImpersonate(requestContext, notificationEventArgs as WorkItemChangedEvent);
                    }

                    EventProcessor eventProcessor = new EventProcessor(uri.AbsoluteUri, toImpersonate, runtime);

                    var context = runtime.RequestContext;
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

        private static string GetServerSettingsFullPath()
        {
            const string PolicyExtension = ".policies";

            var thisAssemblyName = Assembly.GetExecutingAssembly().GetName();

            // Load the options from file with same name as DLL
            string baseName = thisAssemblyName.Name;

            // Load the file from same folder where DLL is located
            return Path.Combine(
                        Path.GetDirectoryName(new Uri(thisAssemblyName.CodeBase).LocalPath),
                        baseName)
                    + PolicyExtension;
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

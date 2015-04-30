using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.TeamFoundation.Common;
using Microsoft.TeamFoundation.Framework.Server;
using Microsoft.TeamFoundation.WorkItemTracking.Server;
using System.Text;
using Aggregator.Core;
using Aggregator.Core.Facade;
using Aggregator.Core.Configuration;

namespace TFSAggregator.TfsSpecific
{
    using Microsoft.TeamFoundation.Client;
    using Microsoft.TeamFoundation.Framework.Client;
    using Microsoft.TeamFoundation.Framework.Common;

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
            var uri = GetCollectionUriFromContext(requestContext);
            string settingsPath = GetSettingsFullPath();
            // TODO avoid reload every time
            var settings = TFSAggregatorSettings.LoadFromFile(settingsPath);
            var logger = new Aggregator.ServerPlugin.ServerEventLogger(settings.LogLevel);


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

                    result = eventProcessor.ProcessEvent(context, notification);
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
            var thisAssembly = System.Reflection.Assembly.GetExecutingAssembly();

            // Load the options from file with same name as DLL
            string baseName = thisAssembly.GetName().Name;
            string extension = ".policies";

            // Load the file from same folder where DLL is located
            return System.IO.Path.ChangeExtension(
                    System.IO.Path.Combine(
                        System.IO.Path.GetDirectoryName(new Uri(thisAssembly.CodeBase).LocalPath),
                        baseName),
                    extension);
        }

        private Uri GetCollectionUriFromContext(TeamFoundationRequestContext requestContext)
        {
            TeamFoundationLocationService service = requestContext.GetService<TeamFoundationLocationService>();
            return service.GetSelfReferenceUri(requestContext, service.GetDefaultAccessMapping(requestContext));
        }


        private IdentityDescriptor GetIdentityToImpersonate(TeamFoundationRequestContext requestContext, WorkItemChangedEvent workItemChangedEvent)
        {
            Uri server = GetCollectionUriFromContext(requestContext);
            
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

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.TeamFoundation.Common;
using Microsoft.TeamFoundation.Framework.Server;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using Microsoft.TeamFoundation.WorkItemTracking.Server;
using System.Text;
using TFSAggregator.ConfigTypes;
using WorkItem = Microsoft.TeamFoundation.WorkItemTracking.Client.WorkItem;
using TFSAggregator.TfsFacade;

namespace TFSAggregator.TfsSpecific
{
    using Microsoft.TeamFoundation.Client;

    /// <summary>
    /// The class that subscribes to server side events on the TFS server.
    /// We're only interested in WorkItemChanged events, so we'll filter that out before calling our main logic.
    /// </summary>
    public class WorkItemChangedEventHandler : ISubscriber
    {
        private EventProcessor eventProcessor = new EventProcessor(); //we only need one for the whole app

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
        /// This is the one where all the magic starts.  Main() so to speak.  I will load the settings, connect to tfs and apply the aggregation rules.
        /// </summary>
        public EventNotificationStatus ProcessEvent(TeamFoundationRequestContext requestContext, NotificationType notificationType, object notificationEventArgs,
                                                    out int statusCode, out string statusMessage, out ExceptionPropertyCollection properties)
        {
            //Abort for non-configured ProjectCollection:
            //Abort early if there is a context mismatch
            if (ShouldAbort(requestContext, new Uri(TFSAggregatorSettings.TFSUri)))
            {
                statusCode = 0;
                statusMessage = "";
                properties = new ExceptionPropertyCollection();
                return EventNotificationStatus.ActionPermitted;
            }

            var result = new ProcessingResult();
            try
            {
                //Check if we have a workitem changed event before proceeding
                if (notificationType == NotificationType.Notification && notificationEventArgs is WorkItemChangedEvent)
                {
                    var context = new RequestContextWrapper(requestContext);
                    var notification = new NotificationWrapper(notificationType, notificationEventArgs as WorkItemChangedEvent);
                    result = eventProcessor.ProcessEvent(context, notification);
                }
            }
            catch (Exception e)
            {
                string message = String.Format("Exception encountered processing notification: {0} \nStack Trace:{1}", e.Message, e.StackTrace);
                if (e.InnerException != null)
                {
                    message += String.Format("\n    Inner Exception: {0} \nStack Trace:{1}", e.InnerException.Message, e.InnerException.StackTrace);
                }
                MiscHelpers.LogMessage(message, true);
            }

            statusCode = result.StatusCode;
            statusMessage = result.StatusMessage;
            properties = result.ExceptionProperties;
            return result.NotificationStatus;
        }

        private bool ShouldAbort(TeamFoundationRequestContext requestContext, Uri configuredUri)
        {
            TeamFoundationLocationService service = requestContext.GetService<TeamFoundationLocationService>();
            Uri selfReferenceUri = service.GetSelfReferenceUri(requestContext, service.GetDefaultAccessMapping(requestContext));
            MiscHelpers.LogMessage("Self reference uri = " + selfReferenceUri, false);
            MiscHelpers.LogMessage("Configured uri = " + configuredUri, false);
            var collection = TfsTeamProjectCollectionFactory.GetTeamProjectCollection(selfReferenceUri);
            return !collection.Uri.AbsoluteUri.Equals(configuredUri.AbsoluteUri, StringComparison.OrdinalIgnoreCase);
        }

        public string Name
        {
            get { return "TFSAggregator"; }
        }

        public SubscriberPriority Priority
        {
            get { return SubscriberPriority.Normal; }
        }
    }
}

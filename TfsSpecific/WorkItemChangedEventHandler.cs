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
using TFSAggregator.TfSFacade;

namespace TFSAggregator.TfsSpecific
{
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
            var status = EventNotificationStatus.ActionPermitted;
            try
            {
                var context = new RequestContext(requestContext);
                var notification = new Notification(notificationType);

                //Check if we have a workitem changed event before proceeding
                if (notificationType != NotificationType.Notification || !(notificationEventArgs is WorkItemChangedEvent))
                {
                    return status;
                }

                ProcessingResult result = eventProcessor.ProcessChangedWorkItem(context, notification, notificationEventArgs);

                status = result.NotificationStatus;
                statusCode = result.StatusCode;
                statusMessage = result.StatusMessage;
                properties = result.Properties;
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
            return status;
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

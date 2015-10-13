using System;
using System.IO;
using System.Linq;
using System.Reflection;

using Aggregator.Core;
using Aggregator.Core.Context;
using Aggregator.Core.Facade;
using Aggregator.Core.Monitoring;
using Aggregator.ServerPlugin;

using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.Common;
using Microsoft.TeamFoundation.Framework.Client;
using Microsoft.TeamFoundation.Framework.Common;
using Microsoft.TeamFoundation.Framework.Server;
using Microsoft.TeamFoundation.WorkItemTracking.Server;

#if TFS2015
using ILocationService = Microsoft.VisualStudio.Services.Location.Server.ILocationService;
#elif TFS2013
using ILocationService = Microsoft.TeamFoundation.Framework.Server.TeamFoundationLocationService;
#endif

namespace TFSAggregator.TfsSpecific
{
    /// <summary>
    /// The class that subscribes to server side events on the TFS server.
    /// We're only interested in WorkItemChanged events, so we'll filter that out before calling our main logic.
    /// </summary>
    public class WorkItemChangedEventHandler : ISubscriber
    {
        public WorkItemChangedEventHandler()
        {
            // DON'T ADD ANYTHING HERE UNLESS YOU REALLY KNOW WHAT YOU ARE DOING.
            // TFS DOES NOT LIKE CONSTRUCTORS HERE AND SEEMS TO FREEZE WHEN YOU TRY :(
        }

        public Type[] SubscribedTypes()
        {
            return new Type[1] { typeof(WorkItemChangedEvent) };
        }

        /// <summary>
        /// This is the one where all the magic starts.  Main() so to speak.  I will load the settings, connect to TFS and apply the aggregation rules.
        /// </summary>
        public EventNotificationStatus ProcessEvent(
#if TFS20151
            IVssRequestContext requestContext,
#else
            TeamFoundationRequestContext context,
#endif
            NotificationType notificationType,
            object notificationEventArgs,
            out int statusCode,
            out string statusMessage,
            out ExceptionPropertyCollection properties)
        {
            var logger = new ServerEventLogger(LogLevel.Normal);
            var context = new RequestContextWrapper(requestContext, notificationType, notificationEventArgs);
            var runtime = RuntimeContext.GetContext(
                GetServerSettingsFullPath,
                context,
                logger,
                (Uri _collectionUri, IdentityDescriptor _toImpersonate, ILogEvents _logger) =>
                    new WorkItemRepository(_collectionUri, _toImpersonate, _logger));

            if (runtime.HasErrors)
            {
                statusCode = 99;
                statusMessage = string.Join(". ", runtime.Errors);
                properties = null;
                return EventNotificationStatus.ActionPermitted;
            }

            var result = new ProcessingResult();
            try
            {
                // Check if we have a workitem changed event before proceeding
                if (notificationType == NotificationType.Notification && notificationEventArgs is WorkItemChangedEvent)
                {
                    using (EventProcessor eventProcessor = new EventProcessor(runtime))
                    {
                        logger.StartingProcessing(context, context.Notification);
                        result = eventProcessor.ProcessEvent(context, context.Notification);
                        logger.ProcessingCompleted(result);
                    }
                }
            }
            catch (Exception e)
            {
                logger.ProcessEventException(e);

                // notify failure
                result.StatusCode = -1;
                result.StatusMessage = "Unexpected error: " + e.Message;
                result.NotificationStatus = EventNotificationStatus.ActionPermitted;
            }

            statusCode = result.StatusCode;
            statusMessage = result.StatusMessage;
            properties = result.ExceptionProperties;
            return result.NotificationStatus;
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

        /// <summary>
        /// Returns the ISubscriber's Name, it's used in logging and the like.
        /// </summary>
        public string Name
        {
            get { return "TFSAggregator2"; }
        }

        /// <summary>
        /// Returns the priority, thi sis used by TFS to decide in which order to run the ISubscriber plugins.
        /// </summary>
        public SubscriberPriority Priority
        {
            get { return SubscriberPriority.Normal; }
        }
    }
}

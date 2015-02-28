using Aggregator.Core;
using Microsoft.TeamFoundation.Framework.Server;
using Microsoft.TeamFoundation.WorkItemTracking.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aggregator.Core.Facade
{
    public class NotificationWrapper : INotification
    {
        private NotificationType notification;
        private WorkItemChangedEvent eventArgs;
        public NotificationWrapper(NotificationType notification, WorkItemChangedEvent eventArgs)
        {
            this.notification = notification;
            this.eventArgs = eventArgs;
        }

        public int WorkItemId
        {
            get
            {
                return eventArgs.CoreFields.IntegerFields[0].NewValue;
            }
        }
    }
}

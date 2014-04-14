using Microsoft.TeamFoundation.Framework.Server;
using Microsoft.TeamFoundation.WorkItemTracking.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace TFSAggregator.TfSFacade
{
    public class Notification
    {
        private NotificationType notification;
        private WorkItemChangedEvent eventArgs;
        public Notification(NotificationType notification, WorkItemChangedEvent eventArgs)
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

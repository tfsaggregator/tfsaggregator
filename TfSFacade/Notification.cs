using Microsoft.TeamFoundation.Framework.Server;
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
        public Notification(NotificationType notification)
        {
            this.notification = notification;
        }


    }
}

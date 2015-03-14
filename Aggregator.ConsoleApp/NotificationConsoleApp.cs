using Aggregator.Core;
using Microsoft.TeamFoundation.Framework.Server;
using Microsoft.TeamFoundation.WorkItemTracking.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aggregator.ConsoleApp
{

    public class NotificationConsoleApp : INotification
    {
        private int workItemId;
        public NotificationConsoleApp(int workItemId)
        {
            this.workItemId = workItemId;
        }

        public int WorkItemId
        {
            get
            {
                return workItemId;
            }
        }
    }
}

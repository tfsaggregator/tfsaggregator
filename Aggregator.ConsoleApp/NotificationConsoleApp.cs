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
        private string projectName;
        public NotificationConsoleApp(int workItemId, string projectName)
        {
            this.workItemId = workItemId;
            this.projectName = projectName;
        }

        public int WorkItemId
        {
            get
            {
                return workItemId;
            }
        }

        public string ProjectUri
        {
            get
            {
                return projectName;
            }
        }
    }
}

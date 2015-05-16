namespace Aggregator.ConsoleApp
{
    using Aggregator.Core;

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

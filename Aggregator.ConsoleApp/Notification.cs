using Aggregator.Core;
using Aggregator.Core.Interfaces;

namespace Aggregator.ConsoleApp
{
    /// <summary>
    /// Notification implementation for the Console Application
    /// </summary>
    public class Notification : INotification
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Notification"/> class.
        /// </summary>
        /// <param name="workItemId">
        /// WorkItemId of the work item to load and apply the policy on.
        /// </param>
        /// <param name="projectName">
        /// The name of the project that holds this work item.
        /// </param>
        public Notification(int workItemId, string projectName)
        {
            this.WorkItemId = workItemId;
            this.ProjectUri = projectName;
        }

        /// <summary>
        /// WorkItemId of the work item to load and apply the policy on.
        /// </summary>
        public int WorkItemId { get; }

        /// <summary>
        /// The name of the project that holds the work item.
        /// </summary>
        public string ProjectUri { get; }
    }
}

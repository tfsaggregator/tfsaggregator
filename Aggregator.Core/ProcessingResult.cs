using System;

using Microsoft.TeamFoundation.Common;
using Microsoft.TeamFoundation.Framework.Server;

namespace Aggregator.Core
{
    /// <summary>
    /// Results and notification returned by <see cref="EventProcessor"/> class.
    /// </summary>
    public class ProcessingResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ProcessingResult"/> class.
        /// </summary>
        public ProcessingResult()
        {
            this.StatusCode = 0;
            this.ExceptionProperties = new ExceptionPropertyCollection();
            this.StatusMessage = string.Empty;
            this.NotificationStatus = EventNotificationStatus.ActionPermitted;
        }

        /// <summary>
        /// </summary>
        public int StatusCode { get; set; }

        /// <summary>
        /// </summary>
        public string StatusMessage { get; set; }

        /// <summary>
        /// </summary>
        public ExceptionPropertyCollection ExceptionProperties { get; set; }

        /// <summary>
        /// </summary>
        public EventNotificationStatus NotificationStatus { get; set; }
    }
}

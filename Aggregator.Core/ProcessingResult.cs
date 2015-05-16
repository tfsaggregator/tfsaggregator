namespace Aggregator.Core
{
    using System;

    using Microsoft.TeamFoundation.Common;
    using Microsoft.TeamFoundation.Framework.Server;

    /// <summary>
    /// Results and notification returned by <see cref="EventProcessor"/> class. 
    /// </summary>
    public class ProcessingResult
    {
        public ProcessingResult()
        {
            StatusCode = 0;
            ExceptionProperties = new ExceptionPropertyCollection();
            StatusMessage = String.Empty;
            NotificationStatus = EventNotificationStatus.ActionPermitted;
        }

        public int StatusCode { get; set; }
        public string StatusMessage { get; set; }
        public ExceptionPropertyCollection ExceptionProperties { get; set; }
        public EventNotificationStatus NotificationStatus { get; set; }
    }
}

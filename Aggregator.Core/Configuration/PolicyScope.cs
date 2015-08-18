using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

using Aggregator.Core.Extensions;
using Aggregator.Core.Interfaces;

namespace Aggregator.Core.Configuration
{
    /// <summary>
    /// Represents a Scope clause of <see cref="TFSAggregatorSettings"/>.
    /// </summary>
    public abstract class PolicyScope
    {
        /// <summary>
        /// Each <see cref="PolicyScope"/> must implement this method to decide whether the policy should activate
        /// for the given requestcontext.
        /// </summary>
        /// <param name="requestContext">The requestcontext of the TFS event</param>
        /// <param name="notification">The notification holding the WorkItemChangedEvent.</param>
        /// <returns>True if the policy should trigger on this request.</returns>
        public abstract bool Matches(IRequestContext requestContext, INotification notification);
    }

    /// <summary>
    /// Implements a <see cref="PolicyScope"/> that allows the user to bind to a specific Process Template name.
    /// </summary>
    /// <remarks>Version checks are not (yet) possible with the On-premise servers. Don't use them for now.</remarks>
    public class TemplateScope : PolicyScope
    {
        private IRequestContext context;

        private INotification notification;

        /// <summary>
        /// The template name to match on.
        /// </summary>
        public string TemplateName { get; set; }

        /// <summary>
        /// The minimum (inclusive) version match on.
        /// </summary>
        /// <remarks>Version checks are not (yet) possible with the On-premise servers. Don't use them for now.</remarks>
        public string MinVersion { get; set; }

        /// <summary>
        /// The maximum (inclusive) version match on.
        /// </summary>
        /// <remarks>Version checks are not (yet) possible with the On-premise servers. Don't use them for now.</remarks>
        public string MaxVersion { get; set; }

        /// <summary>
        /// The Process template id (guid) to match on.
        /// </summary>
        public string TemplateTypeId { get; set; }

        private const string TemplateNamekey = "Process Template";

        /// <summary>
        /// Checks whether this policy matches the request.
        /// </summary>
        /// <param name="requestContext">The requestcontext of the TFS event</param>
        /// <param name="notification">The notification holding the WorkItemChangedEvent.</param>
        /// <returns>true if the policy matches all supplied checks.</returns>
        public override bool Matches(IRequestContext requestContext, INotification notification)
        {
            context = requestContext;
            this.notification = notification;

            return this.MatchesName()
                && this.MatchesId()
                && this.MatchesMinVersion()
                && this.MatchesMaxVersion();
        }

        private bool MatchesMaxVersion()
        {
            if (string.IsNullOrWhiteSpace(this.MaxVersion))
            {
                return true;
            }

            var currentversion = this.context.GetCurrentProjectProcessVersion(new Uri(notification.ProjectUri));

            var current = Version.Parse(string.Format(CultureInfo.InvariantCulture, "{0}.{1}", currentversion.Major, currentversion.Minor));
            var max = Version.Parse(this.MaxVersion);

            return current <= max;
        }

        private bool MatchesMinVersion()
        {
            if (string.IsNullOrWhiteSpace(this.MinVersion))
            {
                return true;
            }

            var currentversion = this.context.GetCurrentProjectProcessVersion(new Uri(notification.ProjectUri));

            var current = Version.Parse(
                string.Format(CultureInfo.InvariantCulture, "{0}.{1}", currentversion.Major, currentversion.Minor));
            var min = Version.Parse(this.MinVersion);

            return current >= min;
        }

        private bool MatchesId()
        {
            if (string.IsNullOrWhiteSpace(this.TemplateTypeId))
            {
                return true;
            }
            var currentversion = this.context.GetCurrentProjectProcessVersion(new Uri(notification.ProjectUri));
            return currentversion.TypeId.Equals(new Guid(this.TemplateTypeId));
        }

        private bool MatchesName()
        {
            if (string.IsNullOrWhiteSpace(this.TemplateName))
            {
                return true;
            }

            IProjectPropertyWrapper[] properties = this.context.GetProjectProperties(new Uri(notification.ProjectUri));
            var templateNameProperty =
                properties.FirstOrDefault(
                    p => string.Equals(TemplateNamekey, p.Name, StringComparison.OrdinalIgnoreCase));

            if (templateNameProperty != null)
            {
                return this.TemplateName.SameAs(templateNameProperty.Value);
            }
            else
            {
                return false;
            }
        }
    }

    /// <summary>
    /// Implements a <see cref="PolicyScope"/> that allows the user to bind to a specific Project collectionName
    /// </summary>
    /// <remarks>Combine with the ProjectScope to scope to a specific project or projects under a collection.</remarks>
    public class CollectionScope : PolicyScope
    {
        /// <summary>
        /// The list of collection names that should execute this policy
        /// </summary>
        public IEnumerable<string> CollectionNames { get; set; }

        public override bool Matches(IRequestContext requestContext, INotification notification)
        {
            return this.CollectionNames.Any(c => requestContext.CollectionName.SameAs(c));
        }
    }

    public class ProjectScope : PolicyScope
    {
        public IEnumerable<string> ProjectNames { get; set; }

        public override bool Matches(IRequestContext requestContext, INotification notification)
        {
            string projectName = requestContext.GetProjectName(new Uri(notification.ProjectUri));
            return this.ProjectNames.Any(c => projectName.SameAs(c));
        }
    }
}

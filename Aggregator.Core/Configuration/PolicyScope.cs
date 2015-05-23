namespace Aggregator.Core.Configuration
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;

    using Aggregator.Core.Interfaces;

    /// <summary>
    /// Represents a Scope clause of <see cref="TFSAggregatorSettings"/>.
    /// </summary>
    public abstract class PolicyScope
    {
        public abstract bool Matches(IRequestContext requestContext, INotification notification);
    }

    public class TemplateScope : PolicyScope
    {
        private IRequestContext context;

        private INotification notification;

        public string TemplateName { get; set; }
        public string MinVersion { get; set; }
        public string MaxVersion { get; set; }

        public string TemplateTypeId { get; set; }

        public const string TemplateNamekey = "Process Template";

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
                
            var current = Version.Parse(
                string.Format(CultureInfo.InvariantCulture, "{0}.{1}", currentversion.Major, currentversion.Minor));
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
  
    public class CollectionScope : PolicyScope
    {
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

using System;
using System.Globalization;
using System.Linq;

using Aggregator.Core.Extensions;
using Aggregator.Core.Interfaces;

namespace Aggregator.Core.Configuration
{
    /// <summary>
    /// Implements a <see cref="PolicyScope"/> that allows the user to bind to a specific Process Template name.
    /// </summary>
    /// <remarks>Version checks are not (yet) possible with the On-premise servers. Don't use them for now.</remarks>
    public class TemplateScope : PolicyScope
    {
        private const string TemplateNameKey = "Process Template";

        /// <summary>
        /// The template name to match on.
        /// </summary>
        public string TemplateName { get; set; }

        public override string DisplayName
        {
            get
            {
                return string.Format("ProcessTemplate({0})", this.TemplateName);
            }
        }

        /// <summary>
        /// Checks whether this policy matches the request.
        /// </summary>
        /// <param name="currentRequestContext">The requestcontext of the TFS event</param>
        /// <param name="currentNotification">The notification holding the WorkItemChangedEvent.</param>
        /// <returns>true if the policy matches all supplied checks.</returns>
        public override ScopeMatchResult Matches(IRequestContext currentRequestContext, INotification currentNotification)
        {
            var res = new ScopeMatchResult();

            IProjectProperty templateNameProperty = GetTemplateInfo(currentRequestContext, currentNotification);

            string processTemplateDescription = string.Format(
                "{0}",
                templateNameProperty?.Value);

            res.Add(processTemplateDescription);

            res.Success = this.MatchesName(templateNameProperty);
            return res;
        }

        private static IProjectProperty GetTemplateInfo(IRequestContext currentRequestContext, INotification currentNotification)
        {
            IProjectProperty[] properties = currentRequestContext.GetProjectProperties(new Uri(currentNotification.ProjectUri));
            var templateNameProperty = properties.FirstOrDefault(
                    p => string.Equals(TemplateNameKey, p.Name, StringComparison.OrdinalIgnoreCase));
            return templateNameProperty;
        }

        private bool MatchesName(IProjectProperty templateNameProperty)
        {
            if (string.IsNullOrWhiteSpace(this.TemplateName))
            {
                return true;
            }

            return this.TemplateName.SameAs(templateNameProperty?.Value);
        }
    }
}
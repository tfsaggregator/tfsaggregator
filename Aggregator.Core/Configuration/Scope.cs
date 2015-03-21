using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Aggregator.Core.Configuration
{
    /// <summary>
    /// Represents a Scope clause of <see cref="TFSAggregatorSettings"/>.
    /// </summary>
    public abstract class Scope
    {
        public abstract bool Matches(IRequestContext requestContext, INotification notification);
    }

    public class TemplateScope : Scope
    {
        public string TemplateName { get; set; }

        public override bool Matches(IRequestContext requestContext, INotification notification)
        {
            // TODO, see http://geekswithblogs.net/TarunArora/archive/2011/11/15/tfs-api-process-template-currently-applied-to-the-team-project.aspx
            throw new NotImplementedException();
        }
    }

    public class CollectionScope : Scope
    {
        public IEnumerable<string> CollectionNames { get; set; }

        public override bool Matches(IRequestContext requestContext, INotification notification)
        {
            return this.CollectionNames.Any(c => requestContext.CollectionName.SameAs(c));
        }
    }

    public class ProjectScope : Scope
    {
        public string CollectionName { get; set; }
        public IEnumerable<string> ProjectNames { get; set; }

        public override bool Matches(IRequestContext requestContext, INotification notification)
        {
            if (!this.CollectionName.SameAs(requestContext.CollectionName))
                return false;
            string projectName = requestContext.GetProjectName(notification.ProjectUri);
            return this.ProjectNames.Any(c => projectName.SameAs(c));
        }
    }
}

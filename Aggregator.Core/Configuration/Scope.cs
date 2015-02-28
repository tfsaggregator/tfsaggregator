using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Aggregator.Core.Configuration
{
    public abstract class Scope
    {
        public abstract bool Matches(IRequestContext requestContext, INotification notification);
    }

    public class TemplateScope : Scope
    {
        public string TemplateName { get; set; }

        public override bool Matches(IRequestContext requestContext, INotification notification)
        {
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
            throw new NotImplementedException();
        }
    }
}

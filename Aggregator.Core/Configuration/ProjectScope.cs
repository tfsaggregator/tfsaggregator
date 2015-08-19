using System;
using System.Collections.Generic;
using System.Linq;

using Aggregator.Core.Extensions;
using Aggregator.Core.Interfaces;

namespace Aggregator.Core.Configuration
{
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
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

        public override string DisplayName
        {
            get
            {
                return string.Format("Projects({0})", string.Join(", ", this.ProjectNames));
            }
        }

        public override ScopeMatchResult Matches(IRequestContext requestContext, INotification notification)
        {
            var res = new ScopeMatchResult();
            string projectName = requestContext.GetProjectName(new Uri(notification.ProjectUri));
            res.Add(projectName);
            res.Success = this.ProjectNames.Any(c => projectName.SameAs(c));
            return res;
        }
    }
}
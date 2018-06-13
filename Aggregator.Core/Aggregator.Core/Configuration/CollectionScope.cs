using System;
using System.Collections.Generic;
using System.Linq;

using Aggregator.Core.Extensions;
using Aggregator.Core.Interfaces;

namespace Aggregator.Core.Configuration
{
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

        public override string DisplayName
        {
            get
            {
                return string.Format("Collections({0})", string.Join(", ", this.CollectionNames));
            }
        }

        public override ScopeMatchResult Matches(IRequestContext requestContext, INotification notification)
        {
            var res = new ScopeMatchResult();
            res.Add(requestContext.CollectionName);
            res.Success = this.CollectionNames.Any(c => requestContext.CollectionName.SameAs(c));
            return res;
        }
    }
}
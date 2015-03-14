using Aggregator.Core;
using Microsoft.TeamFoundation.Framework.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aggregator.ConsoleApp
{
    public class RequestContextConsoleApp : IRequestContext
    {
        internal string teamProjectCollectionUrl;

        public RequestContextConsoleApp(string teamProjectCollectionUrl)
        {
            this.teamProjectCollectionUrl = teamProjectCollectionUrl;
        }

        public string CollectionName
        {
            get
            {
                // HACK
                return this.teamProjectCollectionUrl.Split('/').LastOrDefault();
            }
        }

        public string GetProjectName(string projectName)
        {
            return projectName;
        }
    }
}

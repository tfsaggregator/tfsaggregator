using System;
using System.Linq;

using Aggregator.Core;
using Aggregator.Core.Facade;
using Aggregator.Core.Interfaces;

using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.Framework.Client;
using Microsoft.TeamFoundation.Framework.Common;
using Microsoft.TeamFoundation.Framework.Server;
using Microsoft.TeamFoundation.Server;
using Microsoft.TeamFoundation.Server.Core;
using Microsoft.TeamFoundation.WorkItemTracking.Client;

namespace Aggregator.ConsoleApp
{
    public class RequestContext : IRequestContext
    {
        private readonly string teamProjectCollectionUrl;

        private readonly string teamProjectName;

        public RequestContext(string teamProjectCollectionUrl, string teamProjectName)
        {
            this.teamProjectCollectionUrl = teamProjectCollectionUrl;
            this.teamProjectName = teamProjectName;
        }

        public Uri GetProjectCollectionUri()
        {
            return new Uri(this.teamProjectCollectionUrl);
        }

        public string CollectionName
        {
            get
            {
                var url = new Uri(this.teamProjectCollectionUrl);
                var collection = TfsTeamProjectCollectionFactory.GetTeamProjectCollection(url);
                var name = collection.CatalogNode.Resource.DisplayName;
                return name;
            }
        }

        public INotification Notification
        {
            get
            {
                return new Notification(this.CurrentWorkItemId, this.teamProjectCollectionUrl, this.teamProjectName);
            }
        }

        public int CurrentWorkItemId { get; set; }

        public string GetProjectName(Uri projectUri)
        {
            return this.teamProjectName;
        }

        public IProjectProperty[] GetProjectProperties(Uri projectUri)
        {
            var context = TfsTeamProjectCollectionFactory.GetTeamProjectCollection(new Uri(this.teamProjectCollectionUrl));
            var ics = context.GetService<ICommonStructureService4>();

            string projectName;
            string projectState;
            int templateId = 0;
            ProjectProperty[] projectProperties = null;

            ics.GetProjectProperties(projectUri.ToString(), out projectName, out projectState, out templateId, out projectProperties);

            return projectProperties.Select(p => (IProjectProperty)new ProjectPropertyWrapper() { Name = p.Name, Value = p.Value }).ToArray();
        }

        public IdentityDescriptor GetIdentityToImpersonate()
        {
            // makes no sense impersonation in command line tool... for now
            return null;
        }
    }
}

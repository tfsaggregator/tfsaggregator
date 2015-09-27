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

        public IProjectPropertyWrapper[] GetProjectProperties(Uri projectUri)
        {
            var context = TfsTeamProjectCollectionFactory.GetTeamProjectCollection(new Uri(this.teamProjectCollectionUrl));
            var ics = context.GetService<ICommonStructureService4>();

            string projectName;
            string projectState;
            int templateId = 0;
            ProjectProperty[] projectProperties = null;

            ics.GetProjectProperties(projectUri.ToString(), out projectName, out projectState, out templateId, out projectProperties);

            return projectProperties.Select(p => (IProjectPropertyWrapper)new ProjectPropertyWrapper() { Name = p.Name, Value = p.Value }).ToArray();
        }

        public IProcessTemplateVersionWrapper GetCurrentProjectProcessVersion(Uri projectUri)
        {
            return this.GetProjectProcessVersion(projectUri.AbsoluteUri, ProcessTemplateVersionPropertyNames.CurrentVersion);
        }

        public IProcessTemplateVersionWrapper GetCreationProjectProcessVersion(Uri projectUri)
        {
            return this.GetProjectProcessVersion(projectUri.AbsoluteUri, ProcessTemplateVersionPropertyNames.CreationVersion);
        }

        private IProcessTemplateVersionWrapper GetProjectProcessVersion(string projectUri, string versionPropertyName)
        {
            var context = TfsTeamProjectCollectionFactory.GetTeamProjectCollection(new Uri(this.teamProjectCollectionUrl));
            var ics = context.GetService<ICommonStructureService4>();

            string projectName;
            string projectState;

            int templateId = 0;
            ProjectProperty[] projectProperties = null;

            ics.GetProjectProperties(projectUri, out projectName, out projectState, out templateId, out projectProperties);

            string rawVersion =
                projectProperties.FirstOrDefault(p => p.Name == versionPropertyName)?.Value;

            if (rawVersion == null)
            {
                return new ProcessTemplateVersionWrapper() { TypeId = Guid.Empty, Major = 0, Minor = 0 };
            }
            else
            {
                var result = TeamFoundationSerializationUtility.Deserialize<ProcessTemplateVersion>(rawVersion);
                return new ProcessTemplateVersionWrapper() { TypeId = result.TypeId, Major = result.Major, Minor = result.Minor };
            }
        }

        public IdentityDescriptor GetIdentityToImpersonate()
        {
            // makes no sense impersonation in command line tool... for now
            return null;
        }
    }
}

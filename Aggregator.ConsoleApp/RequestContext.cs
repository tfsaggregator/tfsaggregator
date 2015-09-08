using System;
using System.Linq;

using Aggregator.Core;
using Aggregator.Core.Facade;
using Aggregator.Core.Interfaces;

using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.Framework.Common;
using Microsoft.TeamFoundation.Framework.Server;
using Microsoft.TeamFoundation.Server;
using Microsoft.TeamFoundation.Server.Core;

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

        public string CollectionName
        {
            get
            {
                var context = TfsTeamProjectCollectionFactory.GetTeamProjectCollection(new Uri(this.teamProjectCollectionUrl));
                return context.Name;
            }
        }

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
                projectProperties.FirstOrDefault(p => p.Name == versionPropertyName).Value;

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
    }
}

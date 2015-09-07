using System;
using System.Linq;
using System.Reflection;

using Aggregator.Core.Interfaces;

using Microsoft.TeamFoundation.Framework.Common;
using Microsoft.TeamFoundation.Framework.Server;
using Microsoft.TeamFoundation.Integration.Server;
using Microsoft.TeamFoundation.Server.Core;
#if TFS2015
using Microsoft.VisualStudio.Services.Location.Server;
#endif

using ArtifactPropertyValue = Microsoft.TeamFoundation.Framework.Server.ArtifactPropertyValue;
#if TFS2015
using ILocationService = Microsoft.VisualStudio.Services.Location.Server.ILocationService;
#elif TFS2013
using ILocationService = Microsoft.TeamFoundation.Framework.Server.TeamFoundationLocationService;
#endif
using ArtifactSpec = Microsoft.TeamFoundation.Framework.Server.ArtifactSpec;
using PropertyValue = Microsoft.TeamFoundation.Framework.Server.PropertyValue;

namespace Aggregator.Core.Facade
{
    public class RequestContextWrapper : IRequestContext
    {
        private readonly TeamFoundationRequestContext context;

        public RequestContextWrapper(TeamFoundationRequestContext context)
        {
            this.context = context;
        }

        public string CollectionName => this.context.ServiceHost.Name;

        public string GetProjectName(Uri teamProjectUri)
        {
            var ics = this.context.GetService<ICommonStructureService>();
            string projectName = ics.GetProject(this.context, teamProjectUri.AbsoluteUri).Name;
            return projectName;
        }

        public Uri GetProjectCollectionUri()
        {
            ILocationService service = this.context.GetService<ILocationService>();

            return service.GetSelfReferenceUri(this.context, service.GetDefaultAccessMapping(this.context));
        }

        public IProjectPropertyWrapper[] GetProjectProperties(Uri projectUri)
        {
            var ics = this.context.GetService<ICommonStructureService>();
            string projectName;
            string projectState;

            CommonStructureProjectProperty[] projectProperties = null;

            ics.GetProjectProperties(this.context, projectUri.ToString(), out projectName, out projectState, out projectProperties);

            return projectProperties.Select(p => (IProjectPropertyWrapper)new ProjectPropertyWrapper() { Name = p.Name, Value = p.Value }).ToArray();
        }

        private ArtifactSpec GetProcessTemplateVersionSpec(string projectUri)
        {
            var ics = this.context.GetService<ICommonStructureService>();
            Guid guid = ics.GetProject(this.context, projectUri).ToProjectReference().Id;
            return new ArtifactSpec(ArtifactKinds.ProcessTemplate, guid.ToByteArray(), 0);
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
            ArtifactSpec processTemplateVersionSpec = this.GetProcessTemplateVersionSpec(projectUri);

#if TFS2015
            ProcessTemplateVersion unknown = null;
#elif TFS2013
            ProcessTemplateVersion unknown = ProcessTemplateVersion.Unknown;
#endif
            ProcessTemplateVersion result = unknown;

            using (TeamFoundationDataReader reader = this.context.GetService<TeamFoundationPropertyService>().GetProperties(this.context, processTemplateVersionSpec, new string[] { versionPropertyName }))
            {
                foreach (ArtifactPropertyValue value2 in reader.Cast<ArtifactPropertyValue>())
                {
                    foreach (PropertyValue value3 in value2.PropertyValues)
                    {
                        result =
                            TeamFoundationSerializationUtility.Deserialize<ProcessTemplateVersion>(
                                value3.Value as string);
                        break;
                    }

                    break;
                }
            }

            return result == unknown
                ? new ProcessTemplateVersionWrapper() { TypeId = Guid.Empty, Major = 0, Minor = 0 }
                : new ProcessTemplateVersionWrapper() { TypeId = result.TypeId, Major = result.Major, Minor = result.Minor };
        }
    }
}

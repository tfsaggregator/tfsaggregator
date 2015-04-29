using Aggregator.Core;
using Microsoft.TeamFoundation.Framework.Server;
using Microsoft.TeamFoundation.Integration.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aggregator.Core.Facade
{
    using Aggregator.Core.Interfaces;

    using Microsoft.TeamFoundation.Framework.Common;
    using Microsoft.TeamFoundation.Server;
    using Microsoft.TeamFoundation.Server.Core;

    using ICommonStructureService = Microsoft.TeamFoundation.Integration.Server.ICommonStructureService;

    public class RequestContextWrapper : IRequestContext
    {
        private TeamFoundationRequestContext context;
        public RequestContextWrapper(TeamFoundationRequestContext context)
        {
            this.context = context;
        }

        public string CollectionName
        {
            get
            {
                return context.ServiceHost.Name;
            }
        }

        public string GetProjectName(Uri teamProjectUri)
        {
            // HACK is this cheap?
            var commonService = context.GetService<CommonStructureService>();
            string projectName = commonService.GetProject(context, teamProjectUri.AbsoluteUri).Name;
            return projectName;
        }

        public Uri GetProjectCollectionUri()
        {
            TeamFoundationLocationService service = context.GetService<TeamFoundationLocationService>();
            return service.GetSelfReferenceUri(context, service.GetDefaultAccessMapping(context));
        }

        public IProjectPropertyWrapper[] GetProjectProperties(Uri projectUri)
        {
            var ics = context.GetService<ICommonStructureService>();
            
            string ProjectName = string.Empty; 
            string ProjectState = String.Empty;
            int templateId = 0;
            CommonStructureProjectProperty[] ProjectProperties = null;

            ics.GetProjectProperties(context, projectUri.ToString(), out ProjectName, out ProjectState, out ProjectProperties);
            
            return ProjectProperties.Select(p => (IProjectPropertyWrapper) new ProjectPropertyWrapper(){ Name = p.Name, Value = p.Value} ).ToArray();
        }

        private ArtifactSpec GetProcessTemplateVersionSpec(string projectUri)
        {
            
            var commonService = this.context.GetService<CommonStructureService>();
            Guid guid = commonService.GetProject(this.context, projectUri).ToProjectReference().Id;
            return new ArtifactSpec(ArtifactKinds.ProcessTemplate, guid.ToByteArray(), 0);
        }

        public ProcessTemplateVersion GetCurrentProjectProcessVersion(Uri projectUri)
        {
            return this.GetProjectProcessVersion(projectUri.AbsoluteUri, ProcessTemplateVersionPropertyNames.CurrentVersion);
        }


        public ProcessTemplateVersion GetCreationProjectProcessVersion(Uri projectUri)
        {
            return this.GetProjectProcessVersion(projectUri.AbsoluteUri, ProcessTemplateVersionPropertyNames.CreationVersion);
        }

        private ProcessTemplateVersion GetProjectProcessVersion(string projectUri, string versionPropertyName)
        {
            ArtifactSpec processTemplateVersionSpec = GetProcessTemplateVersionSpec(projectUri);
            ProcessTemplateVersion unknown = ProcessTemplateVersion.Unknown;

            using (TeamFoundationDataReader reader = context.GetService<TeamFoundationPropertyService>().GetProperties(context, processTemplateVersionSpec, new string[] { versionPropertyName }))
            {
                foreach (ArtifactPropertyValue value2 in reader)
                {
                    foreach (PropertyValue value3 in value2.PropertyValues)
                    {
                        return TeamFoundationSerializationUtility.Deserialize<ProcessTemplateVersion>(value3.Value as string);
                    }
                    return unknown;
                }
                return unknown;
            }
        }

    }
}

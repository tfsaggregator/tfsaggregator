namespace Aggregator.Core.Facade
{
    using System;
    using System.Linq;
    using System.Reflection;

    using Aggregator.Core.Interfaces;

    using Microsoft.TeamFoundation.Framework.Common;
    using Microsoft.TeamFoundation.Framework.Server;
    using Microsoft.TeamFoundation.Integration.Server;
    using Microsoft.TeamFoundation.Server.Core;
    using Microsoft.VisualStudio.Services.Location.Server;

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
                return this.context.ServiceHost.Name;
            }
        }

        public string GetProjectName(Uri teamProjectUri)
        {
            // HACK is this cheap?
            var commonService = this.context.GetService<CommonStructureService>();
            string projectName = commonService.GetProject(this.context, teamProjectUri.AbsoluteUri).Name;
            return projectName;
        }

        public Uri GetProjectCollectionUri()
        {
            ILocationService service = this.context.GetService<ILocationService>();
            return service.GetSelfReferenceUri(this.context, service.GetDefaultAccessMapping(this.context));
        }

        public IProjectPropertyWrapper[] GetProjectProperties(Uri projectUri)
        {
            var ics = context.GetService<ICommonStructureService>();
            
            string projectName; 
            string projectState;
            //int templateId = 0;
            CommonStructureProjectProperty[] projectProperties = null;

            ics.GetProjectProperties(this.context, projectUri.ToString(), out projectName, out projectState, out projectProperties);
            
            return projectProperties.Select(p => (IProjectPropertyWrapper) new ProjectPropertyWrapper(){ Name = p.Name, Value = p.Value} ).ToArray();
        }

        private ArtifactSpec GetProcessTemplateVersionSpec(string projectUri)
        {
            
            var commonService = this.context.GetService<CommonStructureService>();
            Guid guid = commonService.GetProject(this.context, projectUri).ToProjectReference().Id;
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
            ProcessTemplateVersion result = null;

            using (TeamFoundationDataReader reader = this.context.GetService<TeamFoundationPropertyService>().GetProperties(this.context, processTemplateVersionSpec, new string[] { versionPropertyName }))
            {
                foreach (ArtifactPropertyValue value2 in reader)
                {
                    foreach (PropertyValue value3 in value2.PropertyValues)
                    {
                        result = TeamFoundationSerializationUtility.Deserialize<ProcessTemplateVersion>(value3.Value as string);
                        break;
                    }
                    break;
                }
            }

            if (result == null)
            {
                return new ProcessTemplateVersionWrapper() { TypeId = Guid.Empty, Major = 0, Minor = 0 };
            }
            else
            {
                return new ProcessTemplateVersionWrapper() { TypeId = result.TypeId, Major = result.Major, Minor = result.Minor };
            }
        }

    }
}

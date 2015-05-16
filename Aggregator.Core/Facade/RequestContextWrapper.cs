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
            TeamFoundationLocationService service = this.context.GetService<TeamFoundationLocationService>();
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
            ArtifactSpec processTemplateVersionSpec = this.GetProcessTemplateVersionSpec(projectUri);
            ProcessTemplateVersion unknown = ProcessTemplateVersion.Unknown;

            using (TeamFoundationDataReader reader = this.context.GetService<TeamFoundationPropertyService>().GetProperties(this.context, processTemplateVersionSpec, new string[] { versionPropertyName }))
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

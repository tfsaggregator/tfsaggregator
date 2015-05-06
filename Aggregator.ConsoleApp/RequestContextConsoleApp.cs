﻿using Aggregator.Core;
using Microsoft.TeamFoundation.Framework.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aggregator.ConsoleApp
{
    using Aggregator.Core.Facade;
    using Aggregator.Core.Interfaces;

    using Microsoft.TeamFoundation.Client;
    using Microsoft.TeamFoundation.Common;
    using Microsoft.TeamFoundation.Framework.Common;
    using Microsoft.TeamFoundation.Integration.Server;
    using Microsoft.TeamFoundation.Server;
    using Microsoft.TeamFoundation.Server.Core;

    public class RequestContextConsoleApp : IRequestContext
    {
        internal string teamProjectCollectionUrl;

        private string teamProjectName;

        public RequestContextConsoleApp(string teamProjectCollectionUrl, string teamProjectName)
        {
            this.teamProjectCollectionUrl = teamProjectCollectionUrl;
            this.teamProjectName = teamProjectName;
        }

        public string CollectionName
        {
            get
            {
                var context = TfsTeamProjectCollectionFactory.GetTeamProjectCollection(new Uri(teamProjectCollectionUrl));
                return context.Name;
            }
        }

        public string GetProjectName(Uri projectUri)
        {
            return teamProjectName;
        }

        public IProjectPropertyWrapper[] GetProjectProperties(Uri projectUri)
        {
            var context = TfsTeamProjectCollectionFactory.GetTeamProjectCollection(new Uri(teamProjectCollectionUrl));
            var ics = context.GetService<ICommonStructureService4>();
            
            string ProjectName = string.Empty;
            string ProjectState = String.Empty;
            int templateId = 0;
            ProjectProperty[] ProjectProperties = null;
            
            ics.GetProjectProperties(projectUri.ToString(), out ProjectName, out ProjectState, out templateId, out ProjectProperties);

            return ProjectProperties.Select(p => (IProjectPropertyWrapper)new ProjectPropertyWrapper() { Name = p.Name, Value = p.Value }).ToArray();
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
            ProcessTemplateVersion unknown = ProcessTemplateVersion.Unknown;

            var context = TfsTeamProjectCollectionFactory.GetTeamProjectCollection(new Uri(teamProjectCollectionUrl));
            var ics = context.GetService<ICommonStructureService4>();

            string ProjectName = string.Empty;
            string ProjectState = String.Empty;
            int templateId = 0;
            ProjectProperty[] ProjectProperties = null;

            ics.GetProjectProperties(projectUri.ToString(), out ProjectName, out ProjectState, out templateId, out ProjectProperties);


            string rawVersion =
                ProjectProperties.FirstOrDefault(p => p.Name == ProcessTemplateVersionPropertyNames.CurrentVersion).Value;

            return TeamFoundationSerializationUtility.Deserialize<ProcessTemplateVersion>(rawVersion);
        }

    }
}
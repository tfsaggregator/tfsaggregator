using System;
using System.Linq;

using Aggregator.Core.Extensions;
using Aggregator.Core.Interfaces;

using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.Framework.Client;
using Microsoft.TeamFoundation.Framework.Common;
using Microsoft.TeamFoundation.Framework.Server;
using Microsoft.TeamFoundation.Integration.Server;
using Microsoft.TeamFoundation.Server.Core;
using Microsoft.TeamFoundation.WorkItemTracking.Server;
#if TFS2015 || TFS2015u1
using Microsoft.VisualStudio.Services.Location.Server;
#endif

#if TFS2015 || TFS2015u1
using ILocationService = Microsoft.VisualStudio.Services.Location.Server.ILocationService;
#elif TFS2013
using ILocationService = Microsoft.TeamFoundation.Framework.Server.TeamFoundationLocationService;
#else
#error Define TFS version!
#endif

#if TFS2015u1
using IVssRequestContext = Microsoft.TeamFoundation.Framework.Server.IVssRequestContext;
#else
using IVssRequestContext = Microsoft.TeamFoundation.Framework.Server.TeamFoundationRequestContext;
#endif

#if TFS2017u2 || TFS2018
using Microsoft.TeamFoundation.Server.Types;
#endif

namespace Aggregator.Core.Facade
{
    public class RequestContextWrapper : IRequestContext, IDisposable
    {
        private readonly IVssRequestContext context;

        public RequestContextWrapper(
            IVssRequestContext context,
            NotificationType notificationType,
            object notificationEventArgs)
        {
            this.context = context;
            this.Notification = new NotificationWrapper(notificationType, notificationEventArgs as WorkItemChangedEvent);
        }

        public string CollectionName => this.context.ServiceHost.Name;

        public INotification Notification
        {
            get;
            private set;
        }

        public IVssRequestContext VssContext
        {
            get
            {
                return this.context;
            }
        }

        public string GetProjectName(Uri teamProjectUri)
        {
            var ics = this.context.GetService<ICommonStructureService>();
            string projectName = ics.GetProject(this.context, teamProjectUri.AbsoluteUri).Name;
            return projectName;
        }

        public Uri GetProjectCollectionUri()
        {
            return this.GetCollectionUriFromContext(this.context);
        }

        public IProjectProperty[] GetProjectProperties(Uri projectUri)
        {
#if TFS2017u2 || TFS2018
            IProjectService projectService = this.context.GetService<IProjectService>();
            var projectId = projectService.GetProjectId(this.context, this.GetProjectName(projectUri));
            var projectProperties = projectService.GetProjectProperties(this.context, projectId, "*");
#else
            var ics = this.context.GetService<ICommonStructureService>();
            string projectName;
            string projectState;

            CommonStructureProjectProperty[] projectProperties;
            ics.GetProjectProperties(this.context, projectUri.ToString(), out projectName, out projectState, out projectProperties);
#endif
            return projectProperties.Select(p => (IProjectProperty)new ProjectPropertyWrapper() { Name = p.Name, Value = p.Value?.ToString() }).ToArray();
        }

        public IdentityDescriptor GetIdentityToImpersonate(Uri projectCollectionUrl)
        {
            var configurationServer = TfsTeamProjectCollectionFactory.GetTeamProjectCollection(projectCollectionUrl);

            // TODO: Find a way to read the identity from the server object model instead.
            IIdentityManagementService identityManagementService =
                configurationServer.GetService<IIdentityManagementService>();

            Microsoft.TeamFoundation.Framework.Client.TeamFoundationIdentity identity =
                identityManagementService.ReadIdentities(
                    new[] { new Guid(this.Notification.ChangerTeamFoundationId) },
                    MembershipQuery.None).FirstOrDefault();

            return identity?.Descriptor;
        }

        private Uri GetCollectionUriFromContext(IVssRequestContext requestContext)
        {
            ILocationService service = requestContext.GetService<ILocationService>();

            return service.GetSelfReferenceUri(requestContext, service.GetDefaultAccessMapping(requestContext));
        }

        public void Dispose()
        {
            this.Dispose(true);
        }

        protected void Dispose(bool disposing)
        {
            if (disposing)
            {
                (this.context as IDisposable)?.Dispose();
            }
        }
    }
}

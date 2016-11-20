using Aggregator.Core.Configuration;
using Aggregator.Core.Context;
using Aggregator.Core.Interfaces;
using Aggregator.Core.Monitoring;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.Framework.Client;
using Microsoft.TeamFoundation.Framework.Common;
using Microsoft.TeamFoundation.Framework.Server;

namespace Aggregator.Core.Facade
{
    public class ScriptLibrary : IScriptLibrary
    {
        private readonly ILogEvents logger;
        private readonly IRequestContext requestContext;
        private readonly ConnectionInfo connectionInfo;

        public ScriptLibrary(IRuntimeContext context)
        {
            this.connectionInfo = context.GetConnectionInfo();
            this.requestContext = context.RequestContext;
            this.logger = context.Logger;
        }

        public void SendMail(string to, string subject, string body)
        {
            var mailService = new TeamFoundationMailService();
            var vssContext = this.requestContext.VssContext;
            mailService.LoadSettings(vssContext);

            string from = mailService.FromAddress.Address;

            this.logger.LibrarySendMail(from, to, subject, body);
            using (var message = new System.Net.Mail.MailMessage(from, to, subject, body))
            {
                mailService.ValidateMessage(vssContext, message);
                mailService.Send(vssContext, message);
            }
        }

        // Get Email Address from TFS Account or Display Name
        // source: https://paulselles.wordpress.com/2014/03/24/tfs-api-tfs-user-email-address-lookup-and-reverse-lookup/
        public string GetEmailAddress(string userName, string defaultValue)
        {
            using (var teamProjectCollection = this.connectionInfo.Token.GetCollection(this.connectionInfo.ProjectCollectionUri))
            {
                var identityManagementService = teamProjectCollection.GetService<IIdentityManagementService>();

                TeamFoundationIdentity identity = identityManagementService.ReadIdentity(
                    IdentitySearchFactor.AccountName,
                    userName,
                    MembershipQuery.None,
                    ReadIdentityOptions.ExtendedProperties);

                // if not found try again using DisplayName
                identity = identity ?? identityManagementService.ReadIdentity(
                    IdentitySearchFactor.DisplayName,
                    userName,
                    MembershipQuery.None,
                    ReadIdentityOptions.ExtendedProperties);

                if (identity == null)
                {
                    return defaultValue;
                }

                // pick first non-null value
                string mailAddress = identity.GetAttribute("Mail", null);
                mailAddress = string.IsNullOrWhiteSpace(mailAddress) ?
                    identity.GetAttribute("ConfirmedNotificationAddress", defaultValue)
                    : mailAddress;

                return mailAddress;
            }
        }
    }
}

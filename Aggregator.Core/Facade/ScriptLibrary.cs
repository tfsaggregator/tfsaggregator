using Aggregator.Core.Context;
using Aggregator.Core.Interfaces;
using Aggregator.Core.Monitoring;
using Microsoft.TeamFoundation.Framework.Server;

namespace Aggregator.Core.Facade
{
    public class ScriptLibrary : IScriptLibrary
    {
        private ILogEvents logger;
        private IRequestContext requestContext;

        public ScriptLibrary(IRuntimeContext context)
        {
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
    }
}

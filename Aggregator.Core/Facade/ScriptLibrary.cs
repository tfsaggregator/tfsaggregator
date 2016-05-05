using Aggregator.Core.Context;
using Aggregator.Core.Interfaces;
using Aggregator.Core.Monitoring;
using Microsoft.TeamFoundation.Framework.Server;

namespace Aggregator.Core.Facade
{
    public class ScriptLibrary : IScriptLibrary
    {
        protected ILogEvents logger;
        protected IRequestContext requestContext;

        internal ScriptLibrary(IRuntimeContext context)
        {
            this.requestContext = context.RequestContext;
            this.logger = context.Logger;
        }

        public void SendMail(string from, string to, string subject, string body)
        {
            this.logger.LibrarySendMail(from, to, subject, body);
            using (var message = new System.Net.Mail.MailMessage(from, to, subject, body))
            {
                var mailService = new TeamFoundationMailService();
                var vssContext = this.requestContext.VssContext;
                mailService.LoadSettings(vssContext);
                mailService.ValidateMessage(vssContext, message);
                mailService.Send(vssContext, message);
            }
        }
    }
}

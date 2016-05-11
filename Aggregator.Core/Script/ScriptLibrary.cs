using System;
using Aggregator.Core.Context;
using Aggregator.Core.Interfaces;
using Aggregator.Core.Monitoring;
using Microsoft.TeamFoundation.Framework.Server;

namespace Aggregator.Core.Script
{
    public class ScriptLibrary : IScriptLibrary
    {
        private readonly ILogEvents logger;
        private readonly IRequestContext requestContext;

        public ScriptLibrary(IRuntimeContext context)
        {
            this.requestContext = context.RequestContext;
            this.logger = context.Logger;
        }

        public void SendMail(string to, string subject, string body)
        {
            string from = "tfsaggregator@example.com";

            this.logger.LibrarySendMail(from, to, subject, body);
            using (var message = new System.Net.Mail.MailMessage(from, to, subject, body))
            {
                using (var smtp = new System.Net.Mail.SmtpClient())
                {
                    smtp.Host = "localhost";
                    smtp.Port = 25;
                    smtp.Send(message);
                }
            }
        }
    }
}

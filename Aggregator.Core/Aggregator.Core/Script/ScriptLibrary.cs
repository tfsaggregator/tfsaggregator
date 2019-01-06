using System;
using Aggregator.Core.Context;
using Aggregator.Core.Interfaces;
using Aggregator.Core.Monitoring;
using Microsoft.TeamFoundation.Framework.Server;

namespace Aggregator.Core.Script
{
    /// <summary>
    /// This is a fake used by ConsoleApp;
    /// The real one for Plugin and WebService is <seealso cref="Aggregator.Core.Facade.ScriptLibrary"/>.
    /// </summary>
    public class ScriptLibrary : IScriptLibrary
    {
        private readonly ILogEvents logger;
        private readonly IRequestContext requestContext;

        public ScriptLibrary(IRuntimeContext context)
        {
            this.requestContext = context.RequestContext;
            this.logger = context.Logger;
        }

        public string GetEmailAddress(string userName, string defaultValue)
        {
            this.logger.UsingFakeGetEmailAddress(userName, defaultValue);
            return defaultValue;
        }

        public void SendMail(string to, string subject, string body)
        {
            this.logger.UsingFakeSendMail();

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

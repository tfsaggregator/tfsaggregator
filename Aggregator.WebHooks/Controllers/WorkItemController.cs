namespace Aggregator.WebHooks.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Web.Http;
    using Aggregator.Core;
    using Aggregator.Core.Context;
    using Aggregator.Core.Monitoring;
    using Aggregator.Models;
    using Aggregator.WebHooks.Models;
    using Aggregator.WebHooks.Utils;
    using BasicAuthentication.Filters;
    using Newtonsoft.Json.Linq;

    public class WorkItemController : ApiController
    {
        public HttpResponseMessage Get()
        {
            var response = new HttpResponseMessage(HttpStatusCode.OK);
            response.Content = new StringContent($"Hello from TFS Aggregator Web Service, running on {Environment.MachineName}");
            return response;
        }

        // TODO async
        [Authorize] // Require some form of authentication
        public HttpResponseMessage Post([FromBody]JObject payload)
        {
            var request = WorkItemRequest.Parse(payload);

            if (!request.IsValid)
            {
                Log(request.Error);
                return new HttpResponseMessage(HttpStatusCode.BadRequest) { ReasonPhrase = request.Error };
            }

            string policyFilePath = System.Configuration.ConfigurationManager.AppSettings["policyFilePath"];

            // macro expansion to permit multi-tenants
            string policyFile = policyFilePath.WithVar(request);

            // cache requires absolute path
            policyFile = System.Web.Hosting.HostingEnvironment.MapPath(policyFile);
            Debug.Assert(System.IO.File.Exists(policyFile), "Policy file not found.");

            // need a logger to show errors in config file (Catch 22)
            var logger = new AspNetEventLogger(request.EventId, LogLevel.Normal);

            var context = new RequestContext(request.TfsCollectionUri, request.TeamProject);
            var runtime = RuntimeContext.GetContext(
                () => policyFile,
                context,
                logger,
                (runtimeContext) => new Core.Facade.WorkItemRepository(runtimeContext),
                (runtimeContext) => new Core.Script.ScriptLibrary(runtimeContext));
            if (runtime.HasErrors)
            {
                Log(runtime.Errors.Current);
                return new HttpResponseMessage(HttpStatusCode.InternalServerError) { ReasonPhrase = runtime.Errors.Current };
            }

            try
            {
                using (EventProcessor eventProcessor = new EventProcessor(runtime))
                {
                    context.CurrentWorkItemId = request.WorkItemId;
                    context.CurrentChangeType = request.ChangeType;
                    var notification = context.Notification;

                    logger.StartingProcessing(context, notification);
                    var result = eventProcessor.ProcessEvent(context, notification);
                    logger.ProcessingCompleted(result);

                    return result.StatusCode == 0
                        ? new HttpResponseMessage(HttpStatusCode.OK)
                        : new HttpResponseMessage(HttpStatusCode.InternalServerError)
                            {
                                ReasonPhrase = result.StatusMessage
                            };
                }//using
            }
            catch (Exception e)
            {
                logger.ProcessEventException(e);
                // stop at first newline
                string firstLineOfErrorMessage = e.Message
                    .Substring(0,
                        e.Message.IndexOf(Environment.NewLine) > 0
                            ? e.Message.IndexOf(Environment.NewLine)
                            : e.Message.Length);
                return new HttpResponseMessage(HttpStatusCode.InternalServerError)
                {
                    ReasonPhrase = firstLineOfErrorMessage
                };
            }//try
        }

        private void Log(string message)
        {
            Trace.WriteLine(message);
            EventLog.WriteEntry("TFSAggregator", message, EventLogEntryType.Warning, 42);
        }
    }
}

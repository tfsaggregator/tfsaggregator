using Aggregator.Core;
using Aggregator.Core.Context;
using Aggregator.Core.Monitoring;
using Aggregator.Models;
using Aggregator.WebHooks.Models;
using Aggregator.WebHooks.Utils;
using BasicAuthentication.Filters;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace Aggregator.WebHooks.Controllers
{
    public class WorkItemController : ApiController
    {
        public HttpResponseMessage Get()
        {
            var response = new HttpResponseMessage(HttpStatusCode.OK);
            response.Content = new StringContent($"Hello from TFSAggregator2webHooks @{Environment.MachineName}");
            return response;
        }

        // TODO async
	    [IdentityBasicAuthentication] // Enable authentication via an ASP.NET Identity user name and password
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
            Debug.Assert(System.IO.File.Exists(policyFile));

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

                    if (result.StatusCode == 0)
                    {
                        return new HttpResponseMessage(HttpStatusCode.OK);
                    }
                    else
                    {
                        return new HttpResponseMessage(HttpStatusCode.InternalServerError) { ReasonPhrase = result.StatusMessage };
                    }
                }//using
            }
            catch (Exception e)
            {
                logger.ProcessEventException(e);
                return new HttpResponseMessage(HttpStatusCode.InternalServerError)
                {
                    Content = new StringContent(e.Message)
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

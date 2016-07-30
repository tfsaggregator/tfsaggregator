using Aggregator.Core;
using Aggregator.Core.Context;
using Aggregator.Core.Monitoring;
using Aggregator.Models;
using Aggregator.WebHooks.Utils;
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
        internal class WorkItemRequest
        {
            // pseudo-data
            internal bool IsValid
            {
                get
                {
                    return string.IsNullOrWhiteSpace(this.Error);
                }
            }
            internal string Error { get; private set; }
            // real data
            internal string EventId { get; private set; }
            internal string EventType { get; private set; }
            internal string AccountId { get; private set; }
            public string CollectionId { get; private set; }
            internal int WorkItemId { get; private set; }
            internal string TeamProject { get; private set; }
            internal string TfsCollectionUri { get; private set; }

            private WorkItemRequest()
            {
                this.Error = string.Empty;
            }

            static internal WorkItemRequest Parse(JObject payload)
            {
                var result = new WorkItemRequest();

                if (payload.Property("eventType") == null)
                {
                    result.Error = $"Could not determine event type for message: {payload}";
                }
                else
                {
                    result.EventType = (string)payload["eventType"];
                    result.EventId = (string)payload["id"];

                    // TODO in the future we will use also the Organization level
                    if (payload.Property("resourceContainers") == null)
                    {
                        // bloody Test button
                        result.Error = $"Test button generates bad messages: do not use with this service.";
                    }
                    else
                    {
                        // VSTS sprint 100 or so introduced the Account, but TFS 2015.3 stil lacks it
                        if (payload.SelectToken("resourceContainers.account") == null)
                        {
                            result.CollectionId = (string)payload["resourceContainers"]["collection"]["id"];
                        }
                        else
                        {
                            result.AccountId = (string)payload["resourceContainers"]["account"]["id"];
                        }
                    }

                    result.WorkItemId = (int)payload["resource"]["workItemId"];
                    string fullUrl = (string)payload["resource"]["url"];
                    result.TfsCollectionUri = fullUrl.Substring(0, fullUrl.IndexOf("_apis"));

                    switch (result.EventType)
                    {
                        case "workitem.created":
                            result.TeamProject = (string)payload["resource"]["fields"]["System.TeamProject"];
                            break;
                        case "workitem.updated":
                            result.TeamProject = (string)payload["resource"]["revision"]["fields"]["System.TeamProject"];
                            break;
                        case "workitem.restored":
                            result.TeamProject = (string)payload["resource"]["fields"]["System.TeamProject"];
                            break;
                        case "workitem.deleted":
                            result.TeamProject = (string)payload["resource"]["fields"]["System.TeamProject"];
                            break;
                        default:
                            result.Error = $"Unsupported eventType {result.EventType}";
                            break;
                    }//switch

                }//if
                return result;
            }
        }

        public HttpResponseMessage Get()
        {
            var response = new HttpResponseMessage(HttpStatusCode.OK);
            response.Content = new StringContent($"Hello from TFSAggregator2webHooks @{Environment.MachineName}");
            return response;
        }

        // TODO async
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
                return new HttpResponseMessage(HttpStatusCode.InternalServerError) { ReasonPhrase = runtime.Errors.Current };
            }

            using (EventProcessor eventProcessor = new EventProcessor(runtime))
            {
                try
                {
                    context.CurrentWorkItemId = request.WorkItemId;
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
                }
                catch (Exception e)
                {
                    logger.ProcessEventException(e);
                    return new HttpResponseMessage(HttpStatusCode.InternalServerError) { ReasonPhrase = e.Message };
                }//try
            }//using
        }

        private void Log(string message)
        {
            //HACK: need something better
            Trace.WriteLine(message);
        }
    }
}

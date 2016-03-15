using Aggregator.Core;
using Aggregator.Core.Context;
using Aggregator.Core.Monitoring;
using Aggregator.Models;
using Aggregator.WebHooks.Utils;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
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
            internal string EventId { get; private set; }
            internal string EventType { get; private set; }
            internal int WorkItemId { get; private set; }
            internal string TeamProject { get; private set; }
            internal string TfsCollectionUri { get; private set; }

            internal WorkItemRequest(JObject payload)
            {
                this.EventType = (string)payload["eventType"];
                this.EventId = (string)payload["id"];

                string fullUrl; // work
                switch (this.EventType)
                {
                    case "workitem.created":
                        this.WorkItemId = (int)payload["resource"]["id"];
                        this.TeamProject = (string)payload["resource"]["fields"]["System.TeamProject"];
                        fullUrl = (string)payload["resource"]["url"];
                        this.TfsCollectionUri = fullUrl.Substring(0, fullUrl.IndexOf("_apis"));
                        break;
                    case "workitem.updated":
                        this.WorkItemId = (int)payload["resource"]["id"];
                        this.TeamProject = (string)payload["resource"]["revision"]["fields"]["System.TeamProject"];
                        fullUrl = (string)payload["resource"]["url"];
                        this.TfsCollectionUri = fullUrl.Substring(0, fullUrl.IndexOf("_apis"));
                        break;
                    case "workitem.restored":
                        goto case "workitem.created";
                    case "workitem.deleted":
                        goto case "workitem.created";
                    default:
                        throw new InvalidOperationException("Unsupported eventType " + this.EventType);
                }//switch

            }
        }


        public HttpResponseMessage Post([FromBody]JObject payload)
        {
            if (payload.Property("eventType") == null)
            {
                Log("Could not determine event type for message: {0}", payload);
                return new HttpResponseMessage(HttpStatusCode.BadRequest);
            }

            var request = new WorkItemRequest(payload);
            switch (request.EventType)
            {
                case "workitem.created":
                    ProcessEvent(request);
                    break;
                case "workitem.updated":
                    ProcessEvent(request);
                    break;
                case "workitem.restored":
                    ProcessEvent(request);
                    break;
                case "workitem.deleted":
                    ProcessEvent(request);
                    break;
                default:
                    Log("Unexpected event type {1} for message: {0}", payload, request.EventType);
                    return new HttpResponseMessage(HttpStatusCode.BadRequest);
            }//switch

            return new HttpResponseMessage(HttpStatusCode.OK);
        }

        private int ProcessEvent(WorkItemRequest request)
        {
            //HACK
            string policyFile = "~/App_Data/HelloWorld.policies";

            // cache requires absolute path
            policyFile = System.Web.Hosting.HostingEnvironment.MapPath(policyFile);
            var ok = System.IO.File.Exists(policyFile);

            // need a logger to show errors in config file (Catch 22)
            var logger = new AspNetEventLogger(request.EventId, LogLevel.Normal);

            var context = new RequestContext(request.TfsCollectionUri, request.TeamProject);
            var runtime = RuntimeContext.GetContext(
                () => policyFile,
                context,
                logger,
                (collectionUri, personalToken, logEvents) =>
                    new Core.Facade.WorkItemRepository(collectionUri, (string)personalToken, logEvents));
            if (runtime.HasErrors)
            {
                return 3;
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

                    return result.StatusCode;
                }
                catch (Exception e)
                {
                    logger.ProcessEventException(e);
                    return 1;
                }//try
            }//using
        }

        private void Log(string v, JObject payload, string eventType)
        {
            //TODO Trace
        }

        private void Log(string v, JObject payload)
        {
            //TODO Trace
        }
    }
}

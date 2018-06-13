[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("UnitTest.WebHooks")]

namespace Aggregator.WebHooks.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web;
    using Aggregator.Core.Interfaces;
    using Newtonsoft.Json.Linq;

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

        internal string Error { get; set; }

        // real data
        internal string EventId { get; set; }

        internal string EventType { get; set; }

        // EventType property decoded
        internal ChangeTypes ChangeType { get; set; }

        internal string AccountId { get; set; }

        internal string CollectionId { get; set; }

        internal int WorkItemId { get; set; }

        internal string TeamProject { get; set; }

        internal string TfsCollectionUri { get; set; }

        internal string TeamProjectUri { get; set; }

        internal string ProjectId { get; set; }

        internal string SchemaVersion { get; set; }

        protected WorkItemRequest()
        {
            this.Error = string.Empty;
        }

        internal static WorkItemRequest Parse(JObject payload)
        {
            var result = new WorkItemRequest();

            try
            {
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
                        if (payload.SelectToken("resourceContainers.account") != null)
                        {
                            result.AccountId = (string)payload["resourceContainers"]["account"]["id"];
                        }
                    }

                    if (payload.Property("resourceVersion") == null)
                    {
                        result.Error = $"Missing resourceVersion data.";
                    }
                    else
                    {
                        result.SchemaVersion = (string)payload["resourceVersion"];
                        if (result.SchemaVersion != "1.0")
                        {
                            result.Error = $"Unsupported resourceVersion '{result.SchemaVersion}'.";
                        }
                    }

                    result.CollectionId = (string)payload["resourceContainers"]["collection"]["id"];
                    result.TfsCollectionUri = (string)payload["resourceContainers"]["collection"]["baseUrl"];
                    result.ProjectId = (string)payload["resourceContainers"]["project"]["id"];
                    result.TeamProjectUri = (string)payload["resourceContainers"]["project"]["baseUrl"];
                    // not always true...
                    result.WorkItemId = (int)payload["resource"]["id"];

                    /* TODO
                    ["resource"]["fields"]["System.TeamProject"]
                    rules out using 'Minimal' for 'Resource detail to send'
                    search a solution!!!
                    */
                    switch (result.EventType)
                    {
                        case "workitem.created":
                            result.ChangeType = Core.Interfaces.ChangeTypes.New;
                            if (payload.SelectToken("resource.fields") != null)
                            {
                                // this requires an ALL payload
                                result.TeamProject = (string)payload["resource"]["fields"]["System.TeamProject"];
                            }
                            else
                            {
                                result.Error = $"TFS Aggregator requires 'All' for 'Resource details to send'.";
                            }
                            break;
                        case "workitem.updated":
                            result.ChangeType = Core.Interfaces.ChangeTypes.Change;
                            result.WorkItemId = (int)payload["resource"]["workItemId"];
                            if (payload.SelectToken("resource.revision.fields") != null)
                            {
                                // this requires an ALL payload
                                result.TeamProject = (string)payload["resource"]["revision"]["fields"]["System.TeamProject"];
                            }
                            else
                            {
                                result.Error = $"TFS Aggregator requires 'All' for 'Resource details to send'.";
                            }
                            break;
                        case "workitem.restored":
                            result.ChangeType = Core.Interfaces.ChangeTypes.Restore;
                            if (payload.SelectToken("resource.fields") != null)
                            {
                                // this requires an ALL payload
                                result.TeamProject = (string)payload["resource"]["fields"]["System.TeamProject"];
                            }
                            else
                            {
                                result.Error = $"TFS Aggregator requires 'All' for 'Resource details to send'.";
                            }
                            break;
                        case "workitem.deleted":
                            result.ChangeType = Core.Interfaces.ChangeTypes.Delete;
                            if (payload.SelectToken("resource.fields") != null)
                            {
                                // this requires an ALL payload
                                result.TeamProject = (string)payload["resource"]["fields"]["System.TeamProject"];
                            }
                            else
                            {
                                result.Error = $"TFS Aggregator requires 'All' for 'Resource details to send'.";
                            }
                            break;
                        //TODO case "workitem.comment":
                        default:
                            result.Error = $"Unsupported eventType {result.EventType}";
                            break;
                    }//switch

                }//if
            }
            catch (Exception e)
            {
                if (string.IsNullOrEmpty(result.Error))
                {
                    result.Error = $"Failed to parse incoming notification from TFS/VSTS. {e.Message}";
                }
            }

            return result;
        }
    }
}
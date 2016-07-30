## Create subscription
https://www.visualstudio.com/en-us/get-started/integrate/service-hooks/webhooks-and-vso-vs
 - Resource details to send = All
 - URL is `/api/webhooks/incoming/genericjson?code=6e4gkqu54zj5otjlixoth94cka05j9h1`





# Notes
## Project skeleton
https://blogs.msdn.microsoft.com/webdev/2015/11/20/using-asp-net-webhooks-with-ifttt-and-zapier-to-monitor-twitter-and-google-sheets/
https://github.com/aspnet/WebHooks/tree/master/samples/GenericReceivers

## Doc & Schema
https://www.visualstudio.com/integrate/get-started/service-hooks/events#workitem.created

Tip - Install WebEssential to generate class from JSON
http://stackoverflow.com/questions/25247128/consume-atlassian-webhook-with-asp-net-webapi

Message JSon
https://www.timecockpit.com/blog/2015/03/30/Importing-Data-from-Visual-Studio-Online-Using-Web-Hooks

## Remote debug
https://azure.microsoft.com/en-us/documentation/articles/web-sites-dotnet-troubleshoot-visual-studio/
 - Cloud Explorer
 - Select the API App
 - Attach Debugger

# TODO
Ideally a TFS Extension to create all subscription required by Aggregator
https://www.visualstudio.com/en-us/integrate/get-started/service-hooks/create-subscription

## Aggregator required data

Environment (multi-tenant)
1. PolicyFile
2. logger

Request
1. WorkItemId
2. TeamProjectCollectionUrl
3. TeamProjectName


## Logging
https://azure.microsoft.com/en-us/documentation/articles/web-sites-enable-diagnostic-log/

## Authentication
 - Add configuration data
 - Personal token / Alternate credentials

```
var url = new Uri("https://giuliov.visualstudio.com/DefaultCollection/");

NetworkCredential netCred = new NetworkCredential("test-tfsaggregator2-webhooks", "***");
BasicAuthCredential basicCred = new BasicAuthCredential(netCred);
TfsClientCredentials tfsCred = new TfsClientCredentials(basicCred);
tfsCred.AllowInteractive = false;

var tpc = new TfsTeamProjectCollection(url, tfsCred);
tpc.Authenticate();
```

> **HUGE PROBLEM**
> Cannot have multiple output dirs
> See http://stackoverflow.com/questions/33319675/the-codedom-provider-type-microsoft-codedom-providers-dotnetcompilerplatform-cs
> this impact the build, packaging and setup

### OAuth support
Alternative to Personal tokens is implementing OAuth
https://vsooauthclientsample.codeplex.com/
https://www.visualstudio.com/en-us/integrate/get-started/auth/oauth
http://almsports.net/visual-studio-online-extensibility-oauth/1525/


# Design

Configuration on-prem is bound to an Instance with Policies mapping Rules to Collections and Projects

In VSTS there will be an Organization grouping multiple Accounts, an Account limited to a single Collection
(see [this post](https://blogs.msdn.microsoft.com/visualstudioalm/2016/01/11/how-we-plan-to-enable-creating-multiple-collections-per-account/))

The way RuntimeContext and TFSAggregatorSettings are designed can work in this scenario,
by having a configuration/policy file per Organization/tenant.

The problem is `System.Runtime.Caching.Cache` with Azure Storage: we cannot use local file system in web apps.
[This post](http://benfoster.io/blog/monitoring-files-in-azure-blob-storage) shows how to extend the cache to 'monitor' Azure Blob Storage.
Ideal for us, but looking for a more turnkey solution.

[Best practices for private config data and connection strings in configuration in ASP.NET and Azure](http://www.hanselman.com/blog/BestPracticesForPrivateConfigDataAndConnectionStringsInConfigurationInASPNETAndAzure.aspx)

## Open design point
Where to store policies so they work in all these scenarios:
 1. on premises (1 tenant)
 2. on premises (multi-tenant)
 3. Azure (1 tenant)
 4. Azure (multi-tenant)

Use web.config to define the meta-policy:

```
<appSettings>
    <add key="policyFilePath" value="~/App_Data/{CollectionId}.policies" />
</appSettings>
```

valid macros:
 1. `AccountId`
 2. `CollectionId`
 3. `TeamProject`
 4. `TfsCollectionUri`


### Sample
```
{
    "id": "e0793213-0e18-45ef-aee2-108be082e09c",
    "eventType": "workitem.updated",
    "publisherId": "tfs",
    "message": {
        "text": "User Story #2 (Second Child Story) updated by Administrator\r\n(http://tfsonprem:8080/tfs/web/wi.aspx?pcguid=9277182d-3d85-4104-8671-ed35736ded82&id=2)",
        "html": "<a href=\"http://tfsonprem:8080/tfs/web/wi.aspx?pcguid=9277182d-3d85-4104-8671-ed35736ded82&amp;id=2\">User Story #2</a> (Second Child Story) updated by Administrator",
        "markdown": "[User Story #2](http://tfsonprem:8080/tfs/web/wi.aspx?pcguid=9277182d-3d85-4104-8671-ed35736ded82&id=2) (Second Child Story) updated by Administrator"
    },
    "detailedMessage": {
        "text": "User Story #2 (Second Child Story) updated by Administrator\r\n(http://tfsonprem:8080/tfs/web/wi.aspx?pcguid=9277182d-3d85-4104-8671-ed35736ded82&id=2)\r\n\r\n",
        "html": "<a href=\"http://tfsonprem:8080/tfs/web/wi.aspx?pcguid=9277182d-3d85-4104-8671-ed35736ded82&amp;id=2\">User Story #2</a> (Second Child Story) updated by Administrator<ul></ul>",
        "markdown": "[User Story #2](http://tfsonprem:8080/tfs/web/wi.aspx?pcguid=9277182d-3d85-4104-8671-ed35736ded82&id=2) (Second Child Story) updated by Administrator\r\n\r\n"
    },
    "resource": {
        "id": 9,
        "workItemId": 2,
        "rev": 8,
        "revisedBy": {
            "id": "d9d65548-e36f-429a-9ae5-d12fd0b57a99",
            "name": "Administrator <TFSONPREM\\Administrator>",
            "url": "http://tfsonprem:8080/tfs/DefaultCollection/_apis/Identities/d9d65548-e36f-429a-9ae5-d12fd0b57a99"
        },
        "revisedDate": "9999-01-01T00:00:00Z",
        "fields": {
            "System.Rev": {
                "oldValue": 7,
                "newValue": 8
            },
            "System.AuthorizedDate": {
                "oldValue": "2016-07-30T10:52:20.51Z",
                "newValue": "2016-07-30T11:03:37.31Z"
            },
            "System.RevisedDate": {
                "oldValue": "2016-07-30T11:03:37.31Z",
                "newValue": "9999-01-01T00:00:00Z"
            },
            "System.ChangedDate": {
                "oldValue": "2016-07-30T10:52:20.51Z",
                "newValue": "2016-07-30T11:03:37.31Z"
            },
            "System.Watermark": {
                "oldValue": 27,
                "newValue": 28
            },
            "System.Description": {
                "oldValue": "...",
                "newValue": ".."
            }
        },
        "_links": {
            "self": {
                "href": "http://tfsonprem:8080/tfs/DefaultCollection/_apis/wit/workItems/2/updates/9"
            },
            "workItemUpdates": {
                "href": "http://tfsonprem:8080/tfs/DefaultCollection/_apis/wit/workItems/2/updates"
            },
            "parent": {
                "href": "http://tfsonprem:8080/tfs/DefaultCollection/_apis/wit/workItems/2"
            },
            "html": {
                "href": "http://tfsonprem:8080/tfs/web/wi.aspx?pcguid=9277182d-3d85-4104-8671-ed35736ded82&id=2"
            }
        },
        "url": "http://tfsonprem:8080/tfs/DefaultCollection/_apis/wit/workItems/2/updates/9",
        "revision": {
            "id": 2,
            "rev": 8,
            "fields": {
                "System.AreaPath": "AgileTfvc",
                "System.TeamProject": "AgileTfvc",
                "System.IterationPath": "AgileTfvc\\Iteration 1",
                "System.WorkItemType": "User Story",
                "System.State": "Removed",
                "System.Reason": "Removed from the backlog",
                "System.CreatedDate": "2016-07-20T19:22:16.637Z",
                "System.CreatedBy": "Administrator <TFSONPREM\\Administrator>",
                "System.ChangedDate": "2016-07-30T11:03:37.31Z",
                "System.ChangedBy": "Administrator <TFSONPREM\\Administrator>",
                "System.Title": "Second Child Story",
                "Microsoft.VSTS.Scheduling.StoryPoints": 3,
                "Microsoft.VSTS.Common.StateChangeDate": "2016-07-20T21:09:20.44Z",
                "Microsoft.VSTS.Common.Priority": 2,
                "Microsoft.VSTS.Common.ValueArea": "Business",
                "WEF_5F32049B51184FE0A4759E6C54C27520_Kanban.Column.Done": false,
                "System.Description": ".."
            },
            "relations": [
                {
                    "rel": "System.LinkTypes.Hierarchy-Reverse",
                    "url": "http://tfsonprem:8080/tfs/DefaultCollection/_apis/wit/workItems/3",
                    "attributes": {
                        "isLocked": false
                    }
                }
            ],
            "_links": {
                "self": {
                    "href": "http://tfsonprem:8080/tfs/DefaultCollection/_apis/wit/workItems/2/revisions/8"
                },
                "workItemRevisions": {
                    "href": "http://tfsonprem:8080/tfs/DefaultCollection/_apis/wit/workItems/2/revisions"
                },
                "parent": {
                    "href": "http://tfsonprem:8080/tfs/DefaultCollection/_apis/wit/workItems/2"
                }
            },
            "url": "http://tfsonprem:8080/tfs/DefaultCollection/_apis/wit/workItems/2/revisions/8"
        }
    },
    "resourceVersion": "1.0",
    "resourceContainers": {
        "collection": {
            "id": "9277182d-3d85-4104-8671-ed35736ded82"
        },
        "project": {
            "id": "2a545c04-070c-4023-9382-9ca8c5b3ce8f"
        }
    },
    "createdDate": "2016-07-30T11:03:38.454Z"
}
```

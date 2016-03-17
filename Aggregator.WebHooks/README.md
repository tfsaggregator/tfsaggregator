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

## Create subscription
https://www.visualstudio.com/en-us/get-started/integrate/service-hooks/webhooks-and-vso-vs
 - Resource details to send = All
 - URL is `/api/webhooks/incoming/genericjson?code=6e4gkqu54zj5otjlixoth94cka05j9h1`

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


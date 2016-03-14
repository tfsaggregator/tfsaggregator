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

param (
  # source TFS/VSTS
  [Parameter(Mandatory=$true)] 
  [string] $tfsURL,
  [Parameter(Mandatory=$true)] 
  [string] $ProjectName,
  [Parameter(Mandatory=$true)] 
  [string] $PersonalAccessToken,
  # destination TFS Aggregator
  [Parameter(Mandatory=$true)] 
  [string] $aggregatorURL,
  [Parameter(Mandatory=$true)] 
  [string] $aggregatorUsername,
  [Parameter(Mandatory=$true)] 
  [string] $aggregatorPassword
)


$basicAuth = [System.Convert]::ToBase64String([System.Text.Encoding]::UTF8.GetBytes("tfsaggregator:${PersonalAccessToken}"))
$headers = @{Authorization=("Basic ${basicAuth}")}
$APIversion = "api-version=1.0"

Import-Module .\Tunable-SSL-Validator\TunableSSLValidator.psd1

$response = Invoke-RestMethod -Uri "${tfsURL}/_apis/projects/${ProjectName}?${APIversion}" -Method Get -ContentType "application/json" -Headers $headers -Insecure
$projectId = $response.Id

$request = @"
{
  "publisherId": "tfs",
  "eventType": "workitem.created",
  "resourceVersion": "1.0",
  "consumerId": "webHooks",
  "consumerActionId": "httpRequest",
  "publisherInputs": {
  	"projectId": "${projectId}"
  },
  "consumerInputs": {
    "url": "${aggregatorURL}",
    "basicAuthUsername":"${aggregatorUsername}",
    "basicAuthPassword":"${aggregatorPassword}",
    "resourceDetailsToSend":"all",
    "messagesToSend":"none",
    "detailedMessagesToSend":"none"
  }
}
"@
$response = Invoke-RestMethod -Uri "${tfsURL}/_apis/hooks/subscriptions?${APIversion}" -Method Post -Body $request -ContentType "application/json" -Headers $headers -Insecure

$request = @"
{
  "publisherId": "tfs",
  "eventType": "workitem.updated",
  "resourceVersion": "1.0",
  "consumerId": "webHooks",
  "consumerActionId": "httpRequest",
  "publisherInputs": {
  	"projectId": "${projectId}"
  },
  "consumerInputs": {
    "url": "${aggregatorURL}",
    "basicAuthUsername":"${aggregatorUsername}",
    "basicAuthPassword":"${aggregatorPassword}",
    "resourceDetailsToSend":"all",
    "messagesToSend":"none",
    "detailedMessagesToSend":"none"
  }
}
"@
$response = Invoke-RestMethod -Uri "${tfsURL}/_apis/hooks/subscriptions?${APIversion}" -Method Post -Body $request -ContentType "application/json" -Headers $headers -Insecure

<#

# workitem.deleted always fail with
# TF26198: The work item does not exist, or you do not have permission to access it.
#>

$request = @"
{
  "publisherId": "tfs",
  "eventType": "workitem.deleted",
  "resourceVersion": "1.0",
  "consumerId": "webHooks",
  "consumerActionId": "httpRequest",
  "publisherInputs": {
  	"projectId": "${projectId}"
  },
  "consumerInputs": {
    "url": "${aggregatorURL}",
    "basicAuthUsername":"${aggregatorUsername}",
    "basicAuthPassword":"${aggregatorPassword}",
    "messagesToSend":"none",
    "detailedMessagesToSend":"none"
  }
}
"@
$response = Invoke-RestMethod -Uri "${tfsURL}/_apis/hooks/subscriptions?${APIversion}" -Method Post -Body $request -ContentType "application/json" -Headers $headers -Insecure

$request = @"
{
  "publisherId": "tfs",
  "eventType": "workitem.restored",
  "resourceVersion": "1.0",
  "consumerId": "webHooks",
  "consumerActionId": "httpRequest",
  "publisherInputs": {
  	"projectId": "${projectId}"
  },
  "consumerInputs": {
    "url": "${aggregatorURL}",
    "basicAuthUsername":"${aggregatorUsername}",
    "basicAuthPassword":"${aggregatorPassword}",
    "resourceDetailsToSend":"all",
    "messagesToSend":"none",
    "detailedMessagesToSend":"none"
  }
}
"@
$response = Invoke-RestMethod -Uri "${tfsURL}/_apis/hooks/subscriptions?${APIversion}" -Method Post -Body $request -ContentType "application/json" -Headers $headers -Insecure

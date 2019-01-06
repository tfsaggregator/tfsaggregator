param (
  # source TFS/VSTS
  [Parameter(Mandatory=$true)] 
  [string] $tfsURL,
  [Parameter(Mandatory=$true)] 
  [string] $PersonalAccessToken,
  # destination TFS Aggregator
  [Parameter(Mandatory=$true)] 
  [string] $aggregatorURL
)

$basicAuth = [System.Convert]::ToBase64String([System.Text.Encoding]::UTF8.GetBytes("tfsaggregator:${PersonalAccessToken}"))
$headers = @{Authorization=("Basic ${basicAuth}")}
$APIversion = "api-version=1.0"

Import-Module .\Tunable-SSL-Validator\TunableSSLValidator.psd1

$response = Invoke-RestMethod -Uri "${tfsURL}/_apis/hooks/subscriptions?${APIversion}" -Method Get -ContentType "application/json" -Headers $headers -Insecure
$response[0].value | where { $_.consumerInputs.url -eq $aggregatorURL } | foreach {
    Invoke-RestMethod -Uri "$( $_.url )?${APIversion}" -Method Delete -ContentType "application/json" -Headers $headers -Insecure
}
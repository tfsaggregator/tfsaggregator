# CONFIGURATION DATA
## define the deploy source here
$localDeployDir = "C:\Users\Administrator\Desktop\2.2-alpha"
## the Zip file for Web/MSDeploy
$packageName = "TFSAggregator-0.2.2-Debug"
## IIS data
$SiteName = "TFS Aggregator"
$appPoolAccount_User = "$env:COMPUTERNAME\tfsaggregatorservice" # AKA service account
$appPoolAccount_Password = 'Semplice1'
## SSL (if different, e.g. selfsigned, alter the code down below)
$useSSL = $false
$sslCertificateChainFile = "verisign.cer"
$sslCertificateFile = "wildcard.example.com.pfx"
$sslCertPassword = '***'
## DNS data
$dns = "aggregator.local"
$port = 443
$IPv4Address = '10.0.0.101'
$dnsServer = "10.0.0.1"

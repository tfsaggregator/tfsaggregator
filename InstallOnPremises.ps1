#Requires -Version 3.0
Set-StrictMode -Version Latest


# CONFIGURATION DATA
## define the deploy source here
$InstallDir = "F:\tfsaggregator-webhooks"
## IIS data
$SiteName = "TFS Aggregator"
$Port = 8088


# install IIS (you may add or remove modules as needed)
Import-Module ServerManager
Install-WindowsFeature -Name Web-Default-Doc,Web-Dir-Browsing,Web-Http-Errors,Web-Static-Content,Web-Http-Logging,Web-Stat-Compression,Web-Filtering,Web-Mgmt-Console

# install ASP.Net
Install-WindowsFeature -Name NET-Framework-45-ASPNET -IncludeAllSubFeature
Install-WindowsFeature -Name Web-ISAPI-Ext,Web-ISAPI-Filter,Web-Asp-Net45


Import-Module WebAdministration

# remove existing site & app pool
Remove-WebAppPool -Name $SiteName -ErrorAction SilentlyContinue
if (Get-Website | Where-Object { $_.Name -eq $SiteName }) {
   Remove-Website $SiteName
}

# create Application Pool
$appPool = New-WebAppPool -Name $SiteName
$appPool.managedRuntimeVersion ="v4.0"
$appPool.managedPipelineMode = "Integrated"
$appPool | Set-Item

# create Web Site
New-Website -Name $SiteName -ApplicationPool $SiteName -Force -Port $Port -PhysicalPath $InstallDir



# configure logging
$EventLogName = "Application"
## create EventLog source
New-EventLog -LogName $EventLogName -Source "TFSAggregator"
## give permission to account writing, see KB 2028427
wevtutil set-log $EventLogName "/ca:O:BAG:SYD:(A`;`;0x2`;`;`;S-1-15-2-1)(A`;`;0xf0007`;`;`;SY)(A`;`;0x7`;`;`;BA)(A`;`;0x7`;`;`;SO)(A`;`;0x3`;`;`;IU)(A`;`;0x3`;`;`;SU)(A`;`;0x3`;`;`;S-1-5-3)(A`;`;0x3`;`;`;S-1-5-33)(A`;`;0x1`;`;`;S-1-5-32-573)(A`;`;0x3`;`;`;${appPoolAccount_SID})" /ab:true /rt:true
New-Item "${InstallDir}\Logs" -ItemType Directory -Force | Out-Null
ICACLS "${InstallDir}\Logs" /grant "IIS AppPool\${SiteName}:F"


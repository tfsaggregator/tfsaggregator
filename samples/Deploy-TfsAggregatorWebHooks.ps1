$ScriptRoot = $PSScriptRoot
# CONFIGURATION DATA
. $ScriptRoot\InstallData.ps1



# look into the account
$appPoolAccount_Domain = ($appPoolAccount_User -split '\\')[0]
$appPoolAccount_Username = ($appPoolAccount_User -split '\\')[1]

# Create local user to run app, no need if using domain account
if ($appPoolAccount_Domain -eq $env:COMPUTERNAME) {
    $Computer = [ADSI]"WinNT://$env:COMPUTERNAME,Computer"

    $LocalAdmin = $Computer.Create("User", $appPoolAccount_Username)
    $LocalAdmin.SetPassword($appPoolAccount_Password)
    $LocalAdmin.SetInfo()
    $LocalAdmin.FullName = "${SiteName} account"
    $LocalAdmin.UserFlags = 64 + 65536 # i.e. ADS_UF_PASSWD_CANT_CHANGE + ADS_UF_DONT_EXPIRE_PASSWD
    $LocalAdmin.SetInfo()
}

# give service account Logon as Batch privilege (lot of code)
$appPoolAccount_SID = ([wmi] "Win32_userAccount.Domain='${appPoolAccount_Domain}',Name='${appPoolAccount_Username}'").SID
$tempSecEditFile = [System.IO.Path]::GetTempFileName()
secedit.exe /export /cfg "${tempSecEditFile}"
$currentSecEditSetting = ""
foreach ($s in (Get-Content -Path $tempSecEditFile)) {
	if ($s -like "SeServiceLogonRight*") {
		$x = $s.split("=",[System.StringSplitOptions]::RemoveEmptyEntries)
		$currentSecEditSetting = $x[1].Trim()
	}
}
if ($currentSecEditSetting -notlike "*${$appPoolAccount_SID}*") {
	# cannot find appPoolAccount SID in Local Policy, add it
	if ([string]::IsNullOrEmpty($currentSecEditSetting)) {
		$currentSecEditSetting = "*${$appPoolAccount_SID}"
	} else {
		$currentSecEditSetting = "*${$appPoolAccount_SID},${currentSecEditSetting}"
	}	
	$infSecEditFileTemplate = @"
[Unicode]
Unicode=yes
[Version]
signature="`$CHICAGO`$"
Revision=1
[Privilege Rights]
SeServiceLogonRight = ${currentSecEditSetting}
"@
	$infSecEditFileTemplate | Set-Content -Path $tempSecEditFile -Encoding Unicode -Force
	Push-Location (Split-Path $tempSecEditFile)
	try {
		secedit.exe /configure /db "secedit.sdb" /cfg "${tempSecEditFile}" /areas USER_RIGHTS
	} finally {
		Pop-Location
	}
}


# install IIS (you may add or remove modules as needed)
Import-Module ServerManager
Install-WindowsFeature -Name Web-Default-Doc,Web-Dir-Browsing,Web-Http-Errors,Web-Static-Content,Web-Http-Logging,Web-Stat-Compression,Web-Filtering,Web-Mgmt-Console

# install ASP.Net
Install-WindowsFeature -Name NET-Framework-45-ASPNET -IncludeAllSubFeature
Install-WindowsFeature -Name Web-ISAPI-Ext,Web-ISAPI-Filter,Web-Asp-Net45

# install WebPI
$webClient = New-Object -TypeName System.Net.WebClient
$webClient.DownloadFile('http://download.microsoft.com/download/C/F/F/CFF3A0B8-99D4-41A2-AE1A-496C08BEB904/WebPlatformInstaller_amd64_en-US.msi',"${env:TEMP}\WebPlatformInstaller_amd64_en-US.msi")
& cmd /c "msiexec.exe /i ${env:TEMP}\WebPlatformInstaller_amd64_en-US.msi" /qb

# install WebDeploy 3.6
& "C:\Program Files\Microsoft\Web Platform Installer\WebpiCmd-x64.exe" /Install /AcceptEULA "/Products:WDeploy36NoSMO"




#####################
# optional: upgrade .NET Fx to 4.6.1
$webClient.DownloadFile("https://download.microsoft.com/download/E/4/1/E4173890-A24A-4936-9FC9-AF930FE3FA40/NDP461-KB3102436-x86-x64-AllOS-ENU.exe","${env:TEMP}\NDP461-KB3102436-x86-x64-AllOS-ENU.exe")
Start-Process -FilePath "${env:TEMP}\NDP461-KB3102436-x86-x64-AllOS-ENU.exe" -ArgumentList "/q /norestart" -Wait -Verb RunAs
# check if a reboot is pending
if ((Get-ChildItem "HKLM:Software\Microsoft\Windows\CurrentVersion\Component Based Servicing\RebootPending") -or (Get-Item "HKLM:SOFTWARE\Microsoft\Windows\CurrentVersion\WindowsUpdate\Auto Update\RebootRequired")) {
    Restart-Computer -?
}
#####################


Import-Module WebAdministration

# remove existing site & app pool
Remove-WebAppPool -Name $SiteName -ErrorAction SilentlyContinue
if (Get-Website | Where-Object { $_.Name -eq $SiteName }) {
   Remove-Website $SiteName
}

# create Application Pool
$appPool = New-WebAppPool -Name $SiteName
$appPool.processModel.userName = $appPoolAccount_User
$appPool.processModel.password = $appPoolAccount_Password
$appPool.processModel.identityType = "SpecificUser"
$appPool.managedRuntimeVersion ="v4.0"
$appPool.managedPipelineMode = "Integrated"
$appPool | Set-Item

# create Web Site (NOTE harcoded IIS path)
if ($useSSL) {
    New-Website -Name $SiteName -ApplicationPool $appPool.name -Force -HostHeader $dns -Ssl -Port $port -PhysicalPath "C:\inetpub\wwwroot\${SiteName}"
} else {
    New-Website -Name $SiteName -ApplicationPool $appPool.name -Force -HostHeader $dns -Port $port -PhysicalPath "C:\inetpub\wwwroot\${SiteName}"
}

# install SSL Cert & use for the web site
if ($useSSL) {
    Import-Certificate -FilePath "${localDeployDir}\${sslCertificateChainFile}" -CertStoreLocation cert:\LocalMachine\Root
    $cert = Import-PfxCertificate -FilePath "${localDeployDir}\${sslCertificateFile}" -CertStoreLocation cert:\LocalMachine\WebHosting -Password (ConvertTo-SecureString $sslCertPassword -AsPlainText -Force)
    $cert | New-Item -Path "IIS:\SslBindings\0.0.0.0!443!${dns}" -Force
    # optional: reduce information
    Remove-WebConfigurationProperty -PSPath 'MACHINE/WEBROOT/APPHOST' -Filter 'system.webServer/httpProtocol/customHeaders' -Name . -AtElement @{name='X-Powered-By'}
}

# configure logging
$EventLogName = "Application"
## create EventLog source
New-EventLog -LogName $EventLogName -Source "TFSAggregator"
## give permission to account writing, see KB 2028427
wevtutil set-log $EventLogName "/ca:O:BAG:SYD:(A`;`;0x2`;`;`;S-1-15-2-1)(A`;`;0xf0007`;`;`;SY)(A`;`;0x7`;`;`;BA)(A`;`;0x7`;`;`;SO)(A`;`;0x3`;`;`;IU)(A`;`;0x3`;`;`;SU)(A`;`;0x3`;`;`;S-1-5-3)(A`;`;0x3`;`;`;S-1-5-33)(A`;`;0x1`;`;`;S-1-5-32-573)(A`;`;0x3`;`;`;${appPoolAccount_SID})" /ab:true /rt:true
## optional: folder for text log file, in case you configure
New-Item -Path "C:\Log" -ItemType Directory -Force

# deploy web app using WebDeploy
$setParametersFile = "${localDeployDir}\${packageName}.SetParameters.xml"
$websiteNode = Select-Xml -XPath "/parameters/setParameter[@name='IIS Web Application Name']" -Path $setParametersFile
$websiteNode.Node.Value = $SiteName
$websiteNode.Node.OwnerDocument.Save($setParametersFile)

& "${localDeployDir}\${packageName}.deploy.cmd" /Y
# require SSL for this Site (must be after WebDeploy)
if ($useSSL) {
    Set-WebConfigurationProperty -PSPath 'MACHINE/WEBROOT/APPHOST' -location "${SiteName}" -filter 'system.webServer/security/access' -name 'sslFlags' -value 'Ssl'
}


# optional: add entry in DNS for new site
Import-Module DnsServer
$hostname = $dns.Split('.')[0]
$zone = $dns.Replace($hostname,'').Substring(1)
Add-DnsServerResourceRecordA -IPv4Address $IPv4Address -Name $hostname -ZoneName $zone -ComputerName $dnsServer

# optional: add entry in hosts file
Add-Content -Path "${env:SystemRoot}\System32\drivers\etc\hosts" -Value "127.0.0.1     ${dns}" -Encoding "Ascii"


# optional: smoke test that TFS Aggregator web site is up and running
if ($useSSL) {
    [Diagnostics.Process]::Start("https://${dns}:${port}/API/WorkItem")
} else {
    [Diagnostics.Process]::Start("http://${dns}:${port}/API/WorkItem")
}
Write-Host "You should see 'Hello from TFSAggregator2webHooks' in the page"


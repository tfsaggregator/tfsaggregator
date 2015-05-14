<#
	Installs and configure TFS Aggregator on target TFS
#>

# make sure these path are correct for target environment
$TfsFolder = "$env:ProgramFiles\Microsoft Team Foundation Server 12.0"
$VsFolder = "${env:ProgramFiles(x86)}\Microsoft Visual Studio 12.0"

$PluginsFolder = "$TfsFolder\Application Tier\Web Services\bin\Plugins"

# create EventLog source for TFS Aggregator
New-EventLog -LogName "Application" -Source "TFSAggregator" -ErrorAction SilentlyContinue

# install the plug-in
Copy-Item .\TFSAggregator2.*.* $PluginsFolder

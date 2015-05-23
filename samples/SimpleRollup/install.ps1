<#
	Installs and configure TFS Aggregator on target TFS
#>

# make sure these path are correct for target environment
$TfsFolder = "$env:ProgramFiles\Microsoft Team Foundation Server 12.0"
$VsFolder = "${env:ProgramFiles(x86)}\Microsoft Visual Studio 12.0"
$CollectionUrl = "http://localhost:8080/tfs/2013u4Collection"
$ProjectName = "Aggregator"

$witadmin = "$VsFolder\Common7\IDE\witadmin.exe"
$PluginsFolder = "$TfsFolder\Application Tier\Web Services\bin\Plugins"

# create EventLog source for TFS Aggregator
New-EventLog -LogName "Application" -Source "TFSAggregator" -ErrorAction SilentlyContinue

# change CMMI Requirement work item type with roll-up fields
& $witadmin importwitd "/collection:$CollectionUrl" "/p:$ProjectName" /f:$PSScriptRoot\Requirement.xml

# install the plug-in
Copy-Item $PSScriptRoot\..\..\Aggregator.ServerPlugin\bin\Debug\*.dll $PluginsFolder
Copy-Item $PSScriptRoot\..\..\Aggregator.ServerPlugin\bin\Debug\*.pdb $PluginsFolder
Copy-Item $PSScriptRoot\..\..\UnitTests.Core\ConfigurationsForTests\SimpleRollup.policies $PluginsFolder

<#
	Installs and configure TFS Aggregator on local TFS
	
	The default policy file installed will simply log a message for any workitem processed.
#>

function Get-TeamFoundationServerInstallPath
{
    if (Test-Path "HKLM:\SOFTWARE\Microsoft\TeamFoundationServer") {
        $highestTFSversion = "{0:N1}" -f (
            Get-ChildItem -Path "HKLM:\SOFTWARE\Microsoft\TeamFoundationServer" |
                Split-Path -Leaf |
                foreach { $_ -as [double] } |
                sort -Descending |
                select -First 1)
        $tfsPath = Get-ItemProperty -Path "HKLM:\SOFTWARE\Microsoft\TeamFoundationServer\$highestTFSversion" -Name InstallPath -ErrorAction SilentlyContinue
        if ($tfsPath) {
            $tfsPath.InstallPath
        }
    }#if
}

$ScriptRoot = Split-Path -Parent -Path $MyInvocation.MyCommand.Definition
$TfsFolder = Get-TeamFoundationServerInstallPath

$PluginsFolder = "$TfsFolder\Application Tier\Web Services\bin\Plugins"

$TA2_BinFiles = "TFSAggregator2.Core.dll","TFSAggregator2.ServerPlugin.dll"
$TA2_SymbolFiles = "TFSAggregator2.Core.pdb","TFSAggregator2.ServerPlugin.pdb"
$TA2_ConfigFiles = "samples\TFSAggregator2.ServerPlugin.policies"


# create EventLog source for TFS Aggregator
New-EventLog -LogName "Application" -Source "TFSAggregator" -ErrorAction SilentlyContinue

# install the plug-in
Push-Location -Path $ScriptRoot

$FilesToInstall = $TA2_BinFiles + $TA2_SymbolFiles
# do not overwrite existing policy file
if (-not (Test-Path (Join-Path $PluginsFolder -ChildPath (Split-Path -Path $TA2_ConfigFiles -Leaf)))) {
  $FilesToInstall += $TA2_ConfigFiles
}

Copy-Item -Path $FilesToInstall -Destination $PluginsFolder -Force

Pop-Location

<#
	Uninstalls TFS Aggregator from local TFS
	
	Policy file is not removed.
	EventLog source is not deleted.
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


# install the plug-in
Push-Location -Path $PluginsFolder

$FilesToUninstall = $TA2_BinFiles + $TA2_SymbolFiles
Remove-Item -Path $FilesToUninstall

Pop-Location

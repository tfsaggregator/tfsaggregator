Setup & install
================================================

 1. Download and extract the binaries from the latest release
 2. Open up `TFSAggregator2.ServerPlugin.policies` and change the example settings to your actual settings.
    - You will find the [complete syntax](docs/Policy-Syntax.md) and [examples](docs/Policy-Examples.md) following the links.
 3. Test your policy using the command line tool, see [TFS Aggregator Troubleshooting](docs/Troubleshooting.md).
 4. Copy `TFSAggregator2.ServerPlugin.dll`, `TFSAggregator2.Core.dll` and `TFSAggregator2.ServerPlugin.policies` to the plugin location on the Application Tier of your TFS Servers
     - The plugin folder is usually at this path: `C:\Program Files\Microsoft Team Foundation Server 12.0\Application Tier\Web Services\bin\Plugins`
     - You must copy the exact same files on all the TFS Application Tier

TFS detects automatically that a file was copied in and will load it in.


Uninstall
================================================
Remove the `TFSAggregator2.*` file from the plugin location on the Application Tier of your TFS Servers
   - The plugin folder is usually at this path: `C:\Program Files\Microsoft Team Foundation Server 12.0\Application Tier\Web Services\bin\Plugins`

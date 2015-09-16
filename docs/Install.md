Setup & install
================================================

 1. Download and extract the binaries from the latest release
 2. Create `TFSAggregator2.ServerPlugin.policies` using one of the provides examples to build your actual policy.
    - You will find the [complete syntax](Policy-Syntax.md) and [examples](Policy-Examples.md) following the links.
 3. Test your policy using  `TFSAggregator2.ConsoleApp.exe` command line tool, see [TFS Aggregator Troubleshooting](Troubleshooting.md).
 4. Register the EventLog source for TFS Aggregator, using an elevated Powershell prompt, by running
    ```
New-EventLog -LogName "Application" -Source "TFSAggregator"
    ```
 5. Copy `TFSAggregator2.ServerPlugin.dll`, `TFSAggregator2.Core.dll` and `TFSAggregator2.ServerPlugin.policies` to the plugin location on the Application Tier of your TFS Servers:
     - The plugin folder is usually at this path for TFS 2015: `C:\Program Files\Microsoft Team Foundation Server 14.0\Application Tier\Web Services\bin\Plugins`;
     - You must copy the exact same files on all TFS Application Tier servers.

TFS detects automatically that a file was copied in and will load it in.

> Note that the command line tool does not run automatically for work items changed by the policy itself.
> You must re-run the command line tool passing the id of the changed work items.


Uninstall
================================================
Remove the `TFSAggregator2.*` files from the plugin location on the Application Tier of your TFS Servers
   - The plugin folder is usually at this path:
     - TFS 2015: `C:\Program Files\Microsoft Team Foundation Server 14.0\Application Tier\Web Services\bin\Plugins`
     - TFS 2013: `C:\Program Files\Microsoft Team Foundation Server 12.0\Application Tier\Web Services\bin\Plugins`

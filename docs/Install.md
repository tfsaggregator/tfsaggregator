# Automated Setup


## Install script

 1. Download and extract the binaries from the latest release
 2. Run the `install.ps1` script, using an elevated Powershell prompt

A default Policy file is installed without overwriting an existing `TFSAggregator2.ServerPlugin.policies` copy.


## Uninstall script

Run the `uninstall.ps1` script, using an elevated Powershell prompt

> Note:
> 
>  - Policy file is not removed.
>  - EventLog source is not deleted.



# Configure

To configure Aggregator you must deploy new rules in the policy file.
 
 1. Edit a copy of the sample `TFSAggregator2.ServerPlugin.policies` file.
 2. Test your new policy using `TFSAggregator2.ConsoleApp.exe` command line tool
 3. Copy the new file to the plugin folder; usually at this path for TFS 2015:
    `C:\Program Files\Microsoft Team Foundation Server 14.0\Application Tier\Web Services\bin\Plugins` 
 4. Verify that your new policy works; see [TFS Aggregator Troubleshooting](Troubleshooting.md) in case of problems.

 See (Console Application)[Console-App.md] for more information on using the command line tool.


# Manual Setup


## Manual install

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


## Manual Uninstall

Remove the `TFSAggregator2.*` files from the plugin location on the Application Tier of your TFS Servers

The plugin folder is usually at this path:

 - TFS 2015: `C:\Program Files\Microsoft Team Foundation Server 14.0\Application Tier\Web Services\bin\Plugins`
 - TFS 2013: `C:\Program Files\Microsoft Team Foundation Server 12.0\Application Tier\Web Services\bin\Plugins`

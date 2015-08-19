
[![Build status](https://ci.appveyor.com/api/projects/status/github/tfsaggregator/tfsaggregator?svg=true)](https://ci.appveyor.com/project/giuliov/tfsaggregator)
[![Join the chat at https://gitter.im/tfsaggregator/tfsaggregator](https://badges.gitter.im/Join%20Chat.svg)](https://gitter.im/tfsaggregator/tfsaggregator?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge&utm_content=badge)

This server side plugin for TFS 2015 enables dynamic calculation of field values in TFS.
(For example: Dev work + Test Work = Total Work).

## What's new in v2

 * A 'real' Scripting language (C#, VB, Powershell)
 * Scoping allows select which rules apply to which Team Project
 * Enhanced filtering to trigger rules when conditions are met
 * Console application to quickly test new rules
 * Richer logging
 * Test harness and modularity to ease adding new features

Example Uses
================================================

 - Update the state of a Bug, PBI (or any parent) to "In Progress" when a child gets moved to "In Progress"
 - Update the state of a Bug, PBI (or any parent) to "Done" when all children get moved to "Done" or "Removed"
 - Update the "Work Remaining" on a Bug, PBI, etc with the sum of all the Task's "Work Remaining".
 - Update the "Work Remaining" on a Sprint with the sum of all the "Work Remaining" of its grandchildren (i.e. tasks of the PBIs and Bugs in the Sprint).
 - Sum up totals on a single work item (i.e. Dev Estimate + Test Estimate = Total Estimate)


Setup & install
================================================

The [Install](docs/Install.md) file contains the full details to correctly setup Aggregator. The general process is:

 1. Download and extract the binaries from the latest release
 2. Open up `TFSAggregator2.ServerPlugin.policies` and change the example settings to your actual settings.
 3. Test your policy using the command line tool.
 4. Copy `TFSAggregator2.*.dll` and `TFSAggregator2.ServerPlugin.policies` to the plugin location on the Application Tier of your TFS Servers

That is all. TFS will detect that a file was copied in and will load it in.


Troubleshooting
================================================
Is it not working? Here is the troubleshooting and how to get help page: [TFS Aggregator Troubleshooting](docs/Troubleshooting.md)


Migrating from v1
================================================
If you used TFS Aggregator in the past, [here](docs/Upgrade-from-v1.md) are the instructions on switching from older versions.


Build and customize
================================================
We used Visual Studio Community Edition 2013 Update 4 to develop this version.
It requires a number of TFS assemblies that cannot be redistributed. You can find the complete list [here](references/PLACEHOLDER.txt).
More information and useful details on the internal design is [here](docs/Internals.md)


[![Build status](https://ci.appveyor.com/api/projects/status/github/tfsaggregator/tfsaggregator?svg=true)](https://ci.appveyor.com/project/giuliov/tfsaggregator)
[![Join the chat at https://gitter.im/tfsaggregator/tfsaggregator](https://badges.gitter.im/Join%20Chat.svg)](https://gitter.im/tfsaggregator/tfsaggregator?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge&utm_content=badge)

This server side plugin for TFS 2013 and 2015 enables dynamic calculation of field values in TFS.
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

The latest [Install](https://github.com/tfsaggregator/tfsaggregator/tree/8ae990810f580c161247a6f6f4720d9b72d98288) file contains the full details to correctly setup Aggregator. The general process is:

 1. Download and extract the binaries from the latest release
 2. Create `TFSAggregator2.ServerPlugin.policies` (or rename one of the existing samples to get started) and change the example settings to your actual settings.
 3. Test your policy using the command line tool.
 4. Copy `TFSAggregator2.*.dll` and `TFSAggregator2.ServerPlugin.policies` to the plugin location on the Application Tier of your TFS Servers

That is all. TFS will detect that a file was copied in and will load it in.


Troubleshooting
================================================
Is it not working? Here is the troubleshooting and how to get help page: [TFS Aggregator Troubleshooting](docs/Troubleshooting.md)


Migrating from v1
================================================
If you used TFS Aggregator in the past, [here](docs/Upgrade-from-v1.md) are the instructions on switching from older versions.

If you're looking for the latest version of V1 (including a lare number of fixes and security updates), you can still find it [here](https://github.com/tfsaggregator/tfsaggregator/tree/8ae990810f580c161247a6f6f4720d9b72d98288). 

**Note**: we won't provide any further support on this old version. Bt if you have a large investment in the old-style rules, it may provide you a better, stabler version until you're ready to move to V2. 

**Note**: You can run both V1 and V2 side-by-side on the same TFS system, you will have to be extra careful not to create infinite loops though.

Build and customize
================================================
We used Visual Studio Community Edition 2013 Update 4 to develop this version.
It requires a number of TFS assemblies that cannot be redistributed. 

You can find the complete list:

 - 2013: [here](./References/2013/PLACEHOLDER.txt).
 - 2015: [here](./References/2015/PLACEHOLDER.txt).

if you have TFS 2015 or TFS 2013 installed on your development machine, the assemblies for that version will be loaded automatically from the installation folder.

More information and useful details on the internal design is [here](docs/Internals.md)

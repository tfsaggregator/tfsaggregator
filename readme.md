
[![Build status](https://ci.appveyor.com/api/projects/status/github/tfsaggregator/tfsaggregator?svg=true)](https://ci.appveyor.com/project/giuliov/tfsaggregator)

This server side plugin for TFS 2013 enables dynamic calculation of field values in TFS.
(For example: Dev work + Test Work = Total Work).

Example Uses
================================================

 - Update the state of a Bug, PBI (or any parent) to "In Progress" when a child gets moved to "In Progress"
 - Update the state of a Bug, PBI (or any parent) to "Done" when all children get moved to "Done" or "Removed"
 - Update the "Work Remaining" on a Bug, PBI, etc with the sum of all the Task's "Work Remaining".
 - Update the "Work Remaining" on a Sprint with the sum of all the "Work Remaining" of its grandchildren (i.e. tasks of the PBIs and Bugs in the Sprint).
 - Sum up totals on a single work item (ie Dev Estimate + Test Estimate = Total Estimate)

Setup
================================================

 1. Download and extract the binaries from the latest release
 2. Open up AggregatorItems.xml and change the example settings to your actual settings.
    - See AggregatorItems.xml Options below to find out how to correctly customize your AggregatorItems.xml file to fit your needs.

Installation
================================================

 1. Copy `Aggregator.ServerPlugin.dll` and `Aggregator.ServerPlugin.policies` to the plugin location on the Application Tier of your TFS Server
     - The plugin folder is usually at this path: `C:\Program Files\Microsoft Team Foundation Server 12.0\Application Tier\Web Services\bin\Plugins`


That is all. TFS will detect that a file was copied in and will load it in.

Troubleshooting
================================================
Is it not working? Here is the troubleshooting and how to get help page: [TFS Aggregator Troubleshooting](docs/Troubleshooting.md)


Aggregator.ServerPlugin.policies Options
================================================
See the [Syntax](docs/Syntax.md) page.
For some [Example Aggregations](docs/Example-Aggregations.md).


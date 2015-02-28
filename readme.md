
[![Build status](https://ci.appveyor.com/api/projects/status/8xecaabbs9r4prmt)](https://ci.appveyor.com/project/giuliov/tfs-aggregator)

This server side plugin for TFS 2010/2012/2013 enables dynamic calculation of field values in TFS.
(For example: Dev work + Test Work = Total Work). It supports same work item and parent-child links.
It also has support for aggregating string values (i.e. Children are Done so the parent is Done).

Because this is a server side plug-in, it is **very fast**.
The very first aggregation takes about 7-10 seconds (as it caches connection information).
After that updates usually take place faster than you can refresh your client.

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

 1. Copy TFSAggregator.dll and AggregatorItems.xml to the plugin location on the Application Tier of your TFS Server
     - The plugin folder is usually at this path: `C:\Program Files\Microsoft Team Foundation Server 12.0\Application Tier\Web Services\bin\Plugins`


That is all. TFS will detect that a file was copied in and will load it in.

Troubleshooting
================================================
Is it not working? Here is the troubleshooting and how to get help page: [TFS Aggregator Troubleshooting](docs/Troubleshooting.md)


AggregatorItems.xml Options
================================================
See the [Syntax](docs/AggregatorItems-Syntax.md) page.
For some [Example Aggregations](docs/Example-Aggregations.md).

Future Features
================================================
These are a list of features I would like to see in TFS Aggregator.
I don't really need them so they have not made the cut yet.
If this turns out to be a tool that others use and they vote for some of the items below then they may get added at some point.

 -  Parent To Child Aggregations.
     -  This would be useful so that when a work item (i.e. PBI/Bug) gets its iteration changed then the TFS Aggregator could change the children to that iteration too.
 -  Make an editor for the options file.
 -  More than one type allowed in workItemType on AggregatorItem.
 -  More support for conditions (allowing differentials). For example having Today - 5 days.
 -  Find a way to filter out repeat messages without having to re-do the aggregation.
    -   A successful aggregations makes changes. That causes the aggregation code to fire again. The second time through it sees that no changes need to be made (because the totals are already right). But it would be nice to have a way to see that up front and not do it again.

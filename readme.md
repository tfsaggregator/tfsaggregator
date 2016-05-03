
[![Build status](https://ci.appveyor.com/api/projects/status/github/tfsaggregator/tfsaggregator?svg=true)](https://ci.appveyor.com/project/giuliov/tfsaggregator)
[![Join the chat at https://gitter.im/tfsaggregator/tfsaggregator](https://badges.gitter.im/Join%20Chat.svg)](https://gitter.im/tfsaggregator/tfsaggregator?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge&utm_content=badge)

This server side plugin for TFS 2013 update 2, TFS 2015 RTM and Update 1 and 2 enables running custom script when Work Items change,
allowing dynamic calculation of field values in TFS and more. (For example: Dev work + Test Work = Total Work).

## What's new in v2

 * A 'real' Scripting language (C#, VB, Powershell)
 * Scoping allows select which rules apply to which Team Project
 * Enhanced filtering to trigger rules when conditions are met
 * Console application to quickly test new rules
 * Richer logging
 * Test harness and modularity to ease adding new features
 * Create new Work Items and Links using rules
 * and more...

Example Uses
================================================

 - Update the state of a Bug, PBI (or any parent) to "In Progress" when a child gets moved to "In Progress"
 - Update the state of a Bug, PBI (or any parent) to "Done" when all children get moved to "Done" or "Removed"
 - Update the "Work Remaining" on a Bug, PBI, etc with the sum of all the Task's "Work Remaining".
 - Update the "Work Remaining" on a Sprint with the sum of all the "Work Remaining" of its grandchildren (i.e. tasks of the PBIs and Bugs in the Sprint).
 - Sum up totals on a single work item (i.e. Dev Estimate + Test Estimate = Total Estimate)

Documentation
================================================

The complete documentation is available on the [project's Wiki](https://github.com/tfsaggregator/tfsaggregator/wiki).

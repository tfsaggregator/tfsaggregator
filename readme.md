![Develop Build status](https://tfsaggregator.visualstudio.com/_apis/public/build/definitions/2e747373-c780-4b2c-823d-98a3fd2b4e99/1/badge)

> # Deprecation notice
> [Azure DevOps Server is deprecating the old work item APIs](https://docs.microsoft.com/en-us/azure/devops/integrate/concepts/wit-client-om-deprecation?WT.mc_id=DOP-MVP-5001511&view=azure-devops). They are likely not going to be removed any time soon, but we can't keep supporting this plugin version forever. 
>
> [Our recomendation is to migrate to aggregator-cli which can now be hosted on Azure Functions or in Docker](https://github.com/tfsaggregator/aggregator-cli).
> 
> We won't do any major investment in this Plugin version and our bandwidth for support is limited.

This server side plugin, for TFS 2013 update 2 up to TFS 2018.3 and Azure DevOps Server 2019 and 2020, enables running custom script when Work Items change,
allowing dynamic calculation of field values in TFS and more. (For example: Dev work + Test Work = Total Work).

# Documentation
The complete documentation is available on the [project's Documentation Site](https://tfsaggregator.github.io/).

# Changelog

## What's new in v2.6
* Adds support for Azure Devops Server 2020 beta, RC and RTW. 

## What's new in v2.5
* Adds support for Azure DevOps Server 2019.0, 2019.0.1, 2019.1RC1, 2019.1RC1, 2019.1
* Adds support for TFS 2018.2 and 2018.3

## What's new in v2.4
* Added support for TFS 2015.4.1
* Added support for TFS 2017.0.1
* Added support for TFS 2017.1
* Added support for TFS 2017.2
* Added support for TFS 2017.3
* Added support for TFS 2017.3.1
* Added support for TFS 2018
* Added support for TFS 2018.1 RC
* Added support for TFS 2018.1
* Added support for TFS 2018.2
* Fixes TemplateScope in TFS 2017 update 2 and higher
* Added text to installer explaining the 2017u2 version works with 2017u3 as well
* Added text to installer explaining the 2018 version works with 2018u1 as well
* Reading and removal of Work item Links self.WorkItemLinks self.RemoveWorkItemLink
* Global List editing with AddItemToGlobalList and RemoveItemFromGlobalList
* Startup logging controlled by configuration file
* New code layout, contributors are urged to read Source Code
* Support for work-item-deleted event

## What's new in v2.2
 * Support for TFS 2017
 * Macro snippets and Functions for Rules and make code more modular
 * Ability to specify server URL
 * Support for multiple workitem Ids in Console application (issue [#178](https://github.com/tfsaggregator/tfsaggregator/issues/178))
 * Ability to Send email from Rules
 * Migrated CI build from AppVeyor to VSTS
 * Use of GitVersion to manage [Semantic Versioning](http://semver.org/)

## What's new in v2.1.1
 * Fixes important bug causing very high CPU usage (see [#160](https://github.com/tfsaggregator/tfsaggregator/issues/160)).

## What's new in v2.1

 * Support for TFS 2015.2, TFS 2015.2.1 and TFS 2015.3
 * Extended logging in debug version
 * Ability to override base Uri of the aggregator
 * Improvements in the setup
 * Adds `PreviousRevision`/`NextRevision` properties to Work Items to navigate history
 * Adds `Uri` field to Work Items
 * Removed policyscope on Workitem template GUID and revision (didn't work anyway)

## What's new in v2

 * A 'real' Scripting language (C#, VB, Powershell)
 * Scoping allows select which rules apply to which Team Project
 * Enhanced filtering to trigger rules when conditions are met
 * Console application to quickly test new rules
 * Richer logging
 * Test harness and modularity to ease adding new features
 * Create new Work Items and Links using rules
 * and more...

## Example Uses

 - Update the state of a Bug, PBI (or any parent) to "In Progress" when a child gets moved to "In Progress"
 - Update the state of a Bug, PBI (or any parent) to "Done" when all children get moved to "Done" or "Removed"
 - Update the "Work Remaining" on a Bug, PBI, etc with the sum of all the Task's "Work Remaining".
 - Update the "Work Remaining" on a Sprint with the sum of all the "Work Remaining" of its grandchildren (i.e. tasks of the PBIs and Bugs in the Sprint).
 - Sum up totals on a single work item (i.e. Dev Estimate + Test Estimate = Total Estimate)
 - Create new work items
 - Create new work item links

# Contributing to the Project

Please read the [Contributing](CONTRIBUTING.md) document.

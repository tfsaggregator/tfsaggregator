###Overview

This is a fork of http://tfsaggregator.codeplex.com.

It enhances the original project in the following ways:

1. Multiple WorkItem Types are now supported. Separate them with a semicolon (;).
2. Added a new operation type **CopyFrom**. Allows you to copy a field value from the parent onto a changed work item.
3. Added a new operation type **CopyTo**. Allows you to change a parent workitem and copy field values into the children.
4. Added 'Source.' and 'Parent.' prefixes to work item fields to support value comparisons between work items when checking Conditions.
5. Renamed TargetItem and SourceItem elements in the config file to TargetField and SourceField to improve readability.

CopyFrom and CopyTo both support the new &lt;OutputFormat formatString="{0}" /&gt; element to determine how the value should be formatted in the taregt item.

In the sample config XML for the CopyFrom and CopyTo operations, a custom field 'Timesheet Job' was added to show how you might use this. The intent would be that in child work items the Timesheet fiels is readonly (except for the TFSService) and channging the timesheet on a feature would cascade changes to the children.

####Installation

Requires TFS2013.

Copy TFSAggregator.dll and AggregatorItems.xml to the plugin folder of your Application Tier. This is usually at C:\Program Files\Microsoft Team Foundation Server 12.0\Application Tier\Web Services\bin\Plugins

Be aware that changing files in the Plugins folder will cause the TFS App Pool to recycle.

####Developers

To build this project you will need to copy a few files from your TFS server (they're non-redistributable, sorry)

You're looking for:
- Microsoft.TeamFoundation.Framework.Server.dll
- Microsoft.TeamFoundation.WorkItemTracking.Server.Dataaccesslayer.dll
- Microsoft.TeamFoundation.WorkItemTracking.Server.dll

For the best development experience, use a TFS2013 VM with VS2013 installed and work directly on the machine.

You can then set the output folder for the project to 
C:\Program Files\Microsoft Team Foundation Server 12.0\Application Tier\Web Services\bin\Plugins\

You can also debug by attaching to the w3wp.exe on the server and setting breakpoints as you would normally.

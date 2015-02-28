So you setup TFS Aggregator and it just sits there.... doing nothing...

Well, this is a check list of things to double check.

 -  You are using it on a TFS 2013 server (and you have the right version for the right server).
 -  Any aggregations between work items have a Parent-Child Link.
 -  You have updated a work item that needs aggregation. (The TFS Aggregation only works once a work item that has aggregation rules on it is updated. This may change in a future version.)
 -  You copied both the DLL and the AggregatorItems.xml file to the plugins location on the TFS Application Tier Server (Usually at: <Drive>`:\Program Files\Microsoft Team Foundation Server 12.0\Application Tier\Web Services\bin\Plugins`)
 -  You have valid names for source and destination fields in AggregatorItems.xml.
 -  When you saved the file you saved it as utf-8 encoding (in Notepad++ it is called “utf-8 without BOM”) (This should not be an issue, but it does not hurt to check).


Also if you are having issues I recommend making a simple AggregatorItems.xml file and trying that out.


Enable Logging
================================================

You can also enable Logging if you are using a TFS 2012 Beta or later version. There are two parts to enable logging.

The first is that you have to add a `loggingVerbosity` attribute to the `AggregatorItems` element in your AggregatorItems.xml file.
There are two valid values, `Normal` and `Diagnostic`. Normal will give you just exception messages, Diagnostic will give you detailed logging.

```
<?xml version="1.0" encoding="utf-8"?>
<AggregatorItems loggingVerbosity="Diagnostic">
```

Then you need to download **DebugView** from Microsoft's SysInternals site.
DebugView is a Trace Listener and will capture the trace messages from TFSAggregator.
> **You have to run DebugView on the TFS App Tier machine**.

I would recommend adding the following filter to DebugView so that you only see the TFSAggregator traces.

```
*TFSAggregator:*
```

Download DebugView at <http://technet.microsoft.com/en-us/sysinternals/bb896647> If you check all the above items and it still does not work then go to https://gist.github.com/ and create gists/posts of your simplified AggregatorItems.xml file and the xml of the work item types you are trying to use TFS Aggregator. Send me the links and a description of the problem and I will try to look into why it does not work (the TFS Power Tools allow you to export the xml of work item types, some simple googling can show you how to do this.).

If you do not want to use Gist-GitHub then you can contact me directly at `TFSAggregatorIssues`<<@>>`spamex.com` (remove the <>).

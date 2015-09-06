So you setup TFS Aggregator and it just sits there.... doing nothing...

Well, this is a check list of things to double check.


## Checklist

 -  You are using it on a TFS 2013 server (and you have the right version for the right server).
 -  Any aggregations between work items have a Parent-Child Link.
 -  You have updated a work item that needs aggregation. (The TFS Aggregation only works once a work item that has aggregation rules on it is updated. This may change in a future version.)
 -  You copied the DLLs and the Policies file to the plugins location on all TFS Application Tier Servers (Usually at: <Drive>`:\Program Files\Microsoft Team Foundation Server 12.0\Application Tier\Web Services\bin\Plugins`)
 -  You have valid names for source and destination fields in `TFSAggregator2.ServerPlugin.policies`.
 -  When you saved the file you saved it as utf-8 encoding (in Notepad++ it is called “utf-8 without BOM”) (This should not be an issue, but it does not hurt to check).
 -  You have given permission to the user running the plugin, i.e. add the "TFS Service Account" to the "Project Collection Administrators" TFS Group.


Also if you are having issues we recommend debugging your Policies file using `TFSAggregator2.ConsoleApp.exe` and trying that out.


## Enable Logging

You can also enable Logging if you are using a TFS 2013 or later version. There are two parts to enable logging.

The first is that you have to set a `level` attribute to the `logging` element in your `TFSAggregator2.ServerPlugin.policies` file.
Use a value like `Verbose` or `Diagnostic`.

```
<?xml version="1.0" encoding="utf-8"?>
<AggregatorConfiguration>
    <runtime>
        <logging level="Diagnostic"/>
    </runtime>
```

Then you need to download **DebugView** from Microsoft's SysInternals site.
DebugView is a Trace Listener and will capture the trace messages from TFSAggregator.
> **You have to run DebugView on _all_ TFS App Tier machines**.

We would recommend adding the following filter to DebugView so that you only see the TFSAggregator traces.

```
*TFSAggregator:*
```

Download DebugView at <http://technet.microsoft.com/en-us/sysinternals/bb896647>.


## Support

If you check all the above items and it still does not work , create a new Issue on GitHub. Please add any useful information like:

 * Aggregator version
 * TFS version
 * Content of `TFSAggregator2.ServerPlugin.policies` file (e.g. save it on https://gist.github.com/ and copy the link in the Issue)
 * Definition of your work item types (use [witadmin exportwitd](https://msdn.microsoft.com/en-us/library/dd312129.aspx))

> **Remove any sensitive information before posting**
  
Consider that this is a voluntary work and we have families and daily jobs, which means: no guarantee of fast response.
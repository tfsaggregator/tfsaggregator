# Use Console Application

The `TFSAggregator2.ConsoleApp.exe` command line tool is extremely useful to test and validate
your policy files before applying to TFS.

> **BEWARE** Any changed workitem is written to TFS database! Use a test TFS instance.


## Syntax

```
 TFSAggregator2.ConsoleApp.exe <command> [<options>]
```
The only supported command is `run`.

If you launch the command without arguments, it will display an help screen. 

## Options
<options> available:

| Option (short form) | Option (long form) | Usage                                                                               |
|---------------------|--------------------|-------------------------------------------------------------------------------------|
| -h  | --help                             | Shows help message and exit                                                         |
| -f  | --policyFile=VALUE                 | Policy file to test                                                                 |
| -c  | --teamProjectCollectionUrl=VALUE   | TFS Team Project Collection Url, e.g. `http://localhost:8080/tfs/DefaultCollection` |
| -p  | --teamProjectName=VALUE            | TFS Team Project                                                                    |
| -n  | --id=VALUE <br> --workItemId=VALUE | Work Item Id                                                                        |
| -l  | --logLevel=VALUE                   | Logging level (critical, error, warning, information, normal, verbose, diagnostic)  |

The log level specified on the command line takes precedence over the level written in the policy file. 


### Sample invocation

```
TFSAggregator2.ConsoleApp.exe run --policyFile=samples\TFSAggregator2.ServerPlugin.policies --teamProjectCollectionUrl=http://localhost:8080/tfs/DefaultCollection --teamProjectName=TfsAggregatorTest1 --workItemId=42 --logLevel=diagnostic
```

### Sample output

The output from the previous invocation should be similar to the following. 
<pre>

Aggregator.ConsoleApp 2.0-beta
Console tool to test TFS Aggregator polices
Build: 2.0.0.0, Configuration: Debug
Copyright (c) TFS Aggregator Team 2015

Executing run (Applies a policy file to specified work item):

[Information] 00.056 Configuration loaded successfully from 'C:\Repos\GitHub\tfsaggregator\samples\TFSAggregator2.ServerPlugin.policies'
<span style="color:blue">[Verbose]     00.058 Building Script Engine for C#</span>
[Information] 00.892 Starting processing on workitem #1
<span style="color:blue">[Verbose]     03.049 Policy 'MyFirstPolicy' applies</span>
<span style="color:blue">[Verbose]     03.050 Evaluating Rule 'MyFirstRule'</span>
[Information] 03.055 Processing completed: Success
<span style="color:green">Succeeded.</span>

</pre>



## Differences from TFS

Here are some major differences.

 * All logging is redirected to the console.
 * If a work item is changed by the rule, it will be processed again by the tool to emulate TFS behavior.
 * The order of processing may be different from TFS.
 * TFS may use different application tier servers to process rules.

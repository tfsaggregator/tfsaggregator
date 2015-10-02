This page explains the design and internal organization of TFS Aggregator v2's code.
If you want to rebuild, customize, submit changes this is the place to start.



# Major Components

The `Aggregator.Core` assembly contains the logic to process a Work Item and run the aggregate scripts.
It is normally used by `Aggregator.ServerPlugin` which intercept the TFS server side events and forward them to Aggregator.Core.
`Aggregator.ConsoleApp` is a simple console app that helps users in developing and testing policies without installing the server plugin.



# Source Code Organization

The project is available on [GitHub](https://github.com/tfsaggregator/tfsaggregator).
We use a simple master/develop/pull-request branching scheme.


## Policy data

Aggregator parses the Policy file at start. The logic is contained in Aggregator.Core/Configuration;
whose entry point is the `Aggregator.Core.Configuration.TFSAggregatorSettings` class.
That class is also the root of the configuration data model: Aggregator code gets a reference to a `TFSAggregatorSettings` instance to configure.

You can populate this class from a different source like a database.


## Object Model

Aggregator's Object Model solves some objectives:
 1. simplifying the scripts
 2. decouple from TFS Object Model
 3. Ease mocking i.e. testing

You find the OM interfaces in Aggregator.Core/Interfaces and the implementation for the TFS OM in Aggregator.Core/Facade.

See [Scripting](Scripting.md) for an introduction.


## Scripting

Aggregator.Core/Script contains the build and execute logic for all scripting engines.
For C# and VB, the script code is compiled once and the produced assembly is reused until the plug-in restarts.
The `DotNetScriptEngine` base class contains all the logic while `CSharpScriptEngine` and `VBNetScriptEngine` define how to sew the script's code snippets toghether.
Powershell support is experimental.


## Logging

The Core is decoupled from the logging sub-system: interesting events are pushed via the `Aggregator.Core.ILogEvents` interface that each client must implement.
This way the same event generate a message in the log file or on the console. Important messages create EventLog messages on the server but not on the console application.

To add a message you have to:
 1. add a method to `ILogEvents` interface
 2. implement the method in `TextLogComposer` class

> Note that the calling site is invoking a method passing typed parameters.
> `TextLogComposer` implementation set the message level and compose the text properly formatting the parameters.



# Build

To rebuild, edit or debug the code you must use Visual Studio 2015, Community or Professional Edition at a minimum.

Building requires a number of TFS assemblies that cannot be redistributed. You can find the complete list 

 - 2013: [here](../References/2013/PLACEHOLDER.txt).
 - 2015: [here](../References/2015/PLACEHOLDER.txt).

if you have TFS 2015 or TFS 2013 installed on your development machine, the assemblies for that version will be loaded automatically from the installation folder.

We use [AppVeyor](http://www.appveyor.com/) for Continuous Integration, so the `appveyor.yml` file is our CI build script.




# Debugging

For the best development experience, use a TFS 2013 or 2015 Virtual Machine with Visual Studio 2015 installed
and work directly on the machine.

You can then set the output folder for the project to
`C:\Program Files\Microsoft Team Foundation Server 12.0\Application Tier\Web Services\bin\Plugins\`
or use the `deploy.cmd` file in _Aggregator.ServerPlugin_ project to refresh Aggregator's assembly on a target test system. Here is a sample

```
@echo off
set CONFIGURATION=%1
set TARGETDIR=%2
set PLUGIN_FOLDER=C:\Program Files\Microsoft Team Foundation Server 14.0\Application Tier\Web Services\bin\Plugins

echo Deploy '%CONFIGURATION%' from '%TARGETDIR%' to '%PLUGIN_FOLDER%'

copy /Y "%TARGETDIR%\TFSAggregator2.Core.dll" "%PLUGIN_FOLDER%"
copy /Y "%TARGETDIR%\TFSAggregator2.Core.pdb" "%PLUGIN_FOLDER%"
copy /Y "%TARGETDIR%\TFSAggregator2.ServerPlugin.dll" "%PLUGIN_FOLDER%"
copy /Y "%TARGETDIR%\TFSAggregator2.ServerPlugin.pdb" "%PLUGIN_FOLDER%"

IF NOT EXIST "%PLUGIN_FOLDER%\TFSAggregator2.ServerPlugin.policies" (
    copy "samples\TFSAggregator2.ServerPlugin.policies" "%PLUGIN_FOLDER%"
)

echo Deploy complete.
```

Do not commit changes to this file!

To debug attach to the `w3wp.exe` on the server and set breakpoints as you would normally.

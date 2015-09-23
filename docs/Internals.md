This page explains some internal design of TFS Aggregator v2.



# Components

The `Aggregator.Core` assembly contains the logic to process a Work Item and run the aggregate scripts.
It is normally used by `Aggregator.ServerPlugin` which intercept the TFS server side events and forward them to Aggregator.Core.
`Aggregator.ConsoleApp` is a simple console app that helps users in developing and testing policies without installing the server plugin.



# Source Code

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
For C# and VB, the script code is compiled once and the produced assembly is reused until the pluging restarts.
The `DotNetScriptEngine` base class containes all the logic while `CSharpScriptEngine` and `VBNetScriptEngine` define how to sew the script's code snippets toghether.
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

You can also debug by attaching to the `w3wp.exe` on the server and setting breakpoints as you would normally.
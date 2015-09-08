This page explains the internal design of TFS Aggregator v2.


# Components

The `Aggregator.Core` assembly contains the logic to aggregate 
It is normally used by `Aggregator.ServerPlugin` which intercept the TFS server side events and forward them to Aggregator.Core.
`Aggregator.ConsoleApp` is a simple console app that helps users in developing and testing policies without installing the server plugin.

# Source Code

## Policy data

TODO
`Aggregator.Core.Configuration.TFSAggregatorSettings` class

## Object Model
See [Scripting](Scripting.md) for an introduction.

TODO

## Scripting

TODO

## Logging

The Core is decoupled from the logging sub-system: interesting events are pushed via the `Aggregator.Core.ILogEvents` interface that each client must implement.

# Build
We used Visual Studio Community Edition 2013 Update 4 and Visual Studio Community Edition 2015 to develop this version.
It requires a number of TFS assemblies that cannot be redistributed. You can find the complete list 

 - 2013: [here](https://github.com/tfsaggregator/tfsaggregator/blob/develop/References/2013/PLACEHOLDER.txt).
 - 2015: [here](https://github.com/tfsaggregator/tfsaggregator/blob/develop/References/2015/PLACEHOLDER.txt).

if you have TFS 2015 or TFS 2013 installed on your development machine, the assemblies for that version will be loaded automatically from the installation folder.

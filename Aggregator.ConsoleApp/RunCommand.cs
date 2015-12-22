using System;
using System.Collections.Generic;

using Aggregator.Core;
using Aggregator.Core.Context;
using Aggregator.Core.Monitoring;

using ManyConsole;

namespace Aggregator.ConsoleApp
{
    /// <summary>
    /// Implements the <i>run</i> command.
    /// </summary>
    /// See ManyConsole framework for more information.
    internal class RunCommand : ConsoleCommand
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RunCommand"/> class.
        /// Informs the ManyConsole framework of the command line arguments required by the  <i>run</i> command.
        /// </summary>
        public RunCommand()
        {
            this.IsCommand("run", "Applies a policy file to specified work item(s)");

            this.HasOption(
                "h|help",
                "Shows this message and exit",
                value => this.ShowHelp = value != null);
            this.HasRequiredOption(
                "f|policyFile=",
                "Policy file to apply",
                value => this.PolicyFile = value);
            this.HasRequiredOption(
                "c|teamProjectCollectionUrl=",
                "TFS Team Project Collection Url, e.g. http://localhost:8080/tfs/DefaultCollection",
                value => this.TeamProjectCollectionUrl = value);
            this.HasRequiredOption(
                "p|teamProjectName=",
                "TFS Team Project",
                value => this.TeamProjectName = value);
            this.HasRequiredOption(
                "n|id|workItemId=",
                "Work Item Id",
                value => this.WorkItemId = int.Parse(value));
            this.HasOption(
                "l|logLevel=",
                "Logging level (critical, error, warning, information, normal, verbose, diagnostic)",
                value =>
                {
                    // use a string but parse so we know it is correct
                    Enum.Parse(typeof(LogLevel), value, true);
                    this.LogLevelName = value;
                });
        }

        internal bool ShowHelp { get; set; }

        internal string PolicyFile { get; set; }

        internal string TeamProjectCollectionUrl { get; set; }

        internal string TeamProjectName { get; set; }

        internal int WorkItemId { get; set; }

        internal string LogLevelName { get; set; }

        /// <summary>
        /// Called by the ManyConsole framework to execute the  <i>run</i> command.
        /// </summary>
        /// <param name="remainingArguments">Unparsed command line arguments.</param>
        /// <returns>0 for success, error code otherwise</returns>
        public override int Run(string[] remainingArguments)
        {
            // cache requires absolute path
            this.PolicyFile = System.IO.Path.GetFullPath(this.PolicyFile);

            // need a logger to show errors in config file (Catch 22)
            var logger = new ConsoleEventLogger(LogLevel.Normal);

            var context = new RequestContext(this.TeamProjectCollectionUrl, this.TeamProjectName);
            var runtime = RuntimeContext.GetContext(
                () => this.PolicyFile,
                context,
                logger,
                (collectionUri, toImpersonate, runtimeContext) =>
                    new Core.Facade.WorkItemRepository(collectionUri, toImpersonate, runtimeContext));

            if (!string.IsNullOrWhiteSpace(this.LogLevelName))
            {
                // command line wins
                LogLevel logLevel = (LogLevel)Enum.Parse(typeof(LogLevel), this.LogLevelName, true);
                runtime.Logger.MinimumLogLevel = logLevel;
            }

            if (runtime.HasErrors)
            {
                return 3;
            }

            using (EventProcessor eventProcessor = new EventProcessor(runtime))
            {
                try
                {
                    var workItemIds = new Queue<int>();
                    workItemIds.Enqueue(this.WorkItemId);

                    ProcessingResult result = null;
                    while (workItemIds.Count > 0)
                    {
                        context.CurrentWorkItemId = workItemIds.Dequeue();
                        var notification = context.Notification;

                        logger.StartingProcessing(context, notification);
                        result = eventProcessor.ProcessEvent(context, notification);
                        logger.ProcessingCompleted(result);

                        foreach (var savedId in eventProcessor.SavedWorkItems)
                        {
                            workItemIds.Enqueue(savedId);
                        }
                    }

                    return result.StatusCode;
                }
                catch (Exception e)
                {
                    logger.ProcessEventException(e);
                    return 1;
                }
            }
        }
    }
}

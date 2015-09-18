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
            this.IsCommand("run", "Applies a policy file to specified work item");

            this.HasOption("h|help", "Shows this message and exit",
              value => this.ShowHelp = value != null);
            this.HasRequiredOption("f|policyFile=", "Policy file to apply",
              value => this.PolicyFile = value);
            this.HasRequiredOption("c|teamProjectCollectionUrl=", "TFS Team Project Collection Url, e.g. http://localhost:8080/tfs/DefaultCollection",
              value => this.TeamProjectCollectionUrl = value);
            this.HasRequiredOption("p|teamProjectName=", "TFS Team Project",
              value => this.TeamProjectName = value);
            this.HasRequiredOption("n|id|workItem=", "Work Item Id",
              value => this.WorkItemId = int.Parse(value));
        }

        internal bool ShowHelp { get; set; }

        internal string PolicyFile { get; set; }

        internal string TeamProjectCollectionUrl { get; set; }

        internal string TeamProjectName { get; set; }

        internal int WorkItemId { get; set; }

        /// <summary>
        /// Called by the ManyConsole framework to execute the  <i>run</i> command.
        /// </summary>
        /// <param name="remainingArguments">Unparsed command line arguments.</param>
        /// <returns>0 for success, error code otherwise</returns>
        public override int Run(string[] remainingArguments)
        {
            // need a logger to show errors in config file (Catch 22)
            var logger = new ConsoleEventLogger(LogLevel.Warning);

            var runtime = RuntimeContext.GetContext(
                () => this.PolicyFile,
                new RequestContext(this.TeamProjectCollectionUrl, this.TeamProjectName),
                logger);

            if (runtime.HasErrors)
            {
                return 99;
            }

            logger.ConfigurationLoaded(this.PolicyFile);
            using (EventProcessor eventProcessor = new EventProcessor(this.TeamProjectCollectionUrl, null, runtime))
            {
                try
                {
                    var workItemIds = new Queue<int>();
                    workItemIds.Enqueue(this.WorkItemId);

                    ProcessingResult result = null;
                    while (workItemIds.Count > 0)
                    {
                        var context = runtime.RequestContext;
                        int id = workItemIds.Dequeue();
                        var notification = new Notification(id, this.TeamProjectName);

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
                    return -1;
                }
            }
        }
    }
}

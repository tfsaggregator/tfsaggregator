using Aggregator.Core;
using Aggregator.Core.Configuration;
using ManyConsole;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aggregator.ConsoleApp
{
    /// <summary>
    /// Implements the <i>run</i> command.
    /// </summary>
    /// See ManyConsole framework for more information.
    class RunCommand : ConsoleCommand
    {
        internal bool ShowHelp { get; set; }
        internal string PolicyFile { get; set; }
        internal string TeamProjectCollectionUrl { get; set; }
        internal string TeamProjectName { get; set; }
        internal int WorkItemId { get; set; }

        /// <summary>
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

        /// <summary>
        /// Called by the ManyConsole framework to execute the  <i>run</i> command.
        /// </summary>
        /// <param name="remainingArguments">Unparsed command line arguments.</param>
        /// <returns>0 for success, error code otherwise</returns>
        public override int Run(string[] remainingArguments)
        {
            // need a logger to show errors in config file (Catch 22)
            var logger = new ConsoleEventLogger(LogLevel.Error);
            var settings = TFSAggregatorSettings.LoadFromFile(this.PolicyFile, logger);
            logger.Level = settings.LogLevel;
            EventProcessor eventProcessor = new EventProcessor(this.TeamProjectCollectionUrl, null, logger, settings); //we only need one for the whole app

            var result = new ProcessingResult();
            try
            {
                var context = new RequestContextConsoleApp(this.TeamProjectCollectionUrl, TeamProjectName);
                var notification = new NotificationConsoleApp(this.WorkItemId, this.TeamProjectName);

                result = eventProcessor.ProcessEvent(context, notification);

                return result.StatusCode;
            }
            catch (Exception e)
            {
                logger.ProcessEventException(null, e);
                return -1;
            }//try
        }
    }
}

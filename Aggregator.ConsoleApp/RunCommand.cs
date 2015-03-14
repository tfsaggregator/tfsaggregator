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
    class RunCommand : ConsoleCommand
    {
        internal bool ShowHelp { get; set; }
        internal string PolicyFile { get; set; }
        internal string TeamProjectCollectionUrl { get; set; }
        internal int WorkItemId { get; set; }

        public RunCommand()
        {
            this.IsCommand("run", "Applies a policy file to specified work item");

            this.HasOption("h|help", "Shows this message and exit",
              value => this.ShowHelp = value != null);
            this.HasRequiredOption("f|p|policyFile=", "Policy file to apply",
              value => this.PolicyFile = value);
            this.HasRequiredOption("c|teamProjectCollectionUrl=", "TFS Team Project Collection Url, e.g. http://localhost:8080/tfs/DefaultCollection",
              value => this.TeamProjectCollectionUrl = value);
            this.HasRequiredOption("n|id|workItem=",  "Work Item Id",
              value => this.WorkItemId = int.Parse(value));
        }

        public override int Run(string[] remainingArguments)
        {
            // the level will change as soon as we get the configuration
            var logger = new ConsoleEventLogger(LogLevel.Warning);
            EventProcessor eventProcessor = new EventProcessor(this.TeamProjectCollectionUrl, logger); //we only need one for the whole app

            var result = new ProcessingResult();
            try
            {
                var context = new RequestContextConsoleApp(this.TeamProjectCollectionUrl);
                var notification = new NotificationConsoleApp(this.WorkItemId);

                var settings = TFSAggregatorSettings.LoadFromFile(this.PolicyFile);

                logger.Level = settings.LogLevel;
                result = eventProcessor.ProcessEvent(context, notification, settings);

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

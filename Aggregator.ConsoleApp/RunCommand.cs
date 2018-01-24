using System;
using System.Collections.Generic;
using System.Linq;

using Aggregator.Core;
using Aggregator.Core.Context;
using Aggregator.Core.Monitoring;

using ManyConsole;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.WorkItemTracking.Client;

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
            this.HasOption(
                "n|id|workItemId=",
                "Work Item Id(s), use comma (,) to separate",
                value => this.WorkItemIds = value.Split(new[] { ',' }).Select(id => int.Parse(id)).ToArray());
            this.HasOption(
                "q|query=",
                "Work Item Query",
                value => this.WorkItemQuery = value);
            this.HasOption(
                "l|logLevel=",
                "Logging level (critical, error, warning, information, normal, verbose, diagnostic)",
                value =>
                {
                    // use a string but parse so we know it is correct
                    Enum.Parse(typeof(LogLevel), value, true);
                    this.LogLevelName = value;
                });
            this.HasOption(
                "t|test|whatIf",
                "Shows this message and exit",
                value => this.WhatIf = value != null);
        }

        internal bool ShowHelp { get; set; }

        internal string PolicyFile { get; set; }

        internal string TeamProjectCollectionUrl { get; set; }

        internal string TeamProjectName { get; set; }

        internal int[] WorkItemIds { get; set; }

        internal string WorkItemQuery { get; set; }

        internal string LogLevelName { get; set; }

        internal bool WhatIf { get; set; }

        public override void CheckRequiredArguments()
        {
            if (string.IsNullOrWhiteSpace(this.WorkItemQuery) && this.WorkItemIds?.Length == 0)
            {
                throw new ConsoleHelpAsException("Specify the work item(s) using Ids or a Query!");
            }

            base.CheckRequiredArguments();
        }

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
                (runtimeContext) => new Core.Facade.WorkItemRepository(runtimeContext),
                (runtimeContext) => new Core.Script.ScriptLibrary(runtimeContext));

            runtime.Settings.WhatIf = this.WhatIf;

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

            var workItemIds = new Queue<int>();
            if (string.IsNullOrWhiteSpace(this.WorkItemQuery))
            {
                foreach (int id in this.WorkItemIds)
                {
                    workItemIds.Enqueue(id);
                }
            }
            else
            {
                var ci = runtime.GetConnectionInfo();
                //HACK should be: ci.ProjectCollectionUri = new Uri(this.TeamProjectCollectionUrl);
                ci.GetType().GetProperty("ProjectCollectionUri").SetValue(ci, new Uri(this.TeamProjectCollectionUrl));
                using (var tfs = ci.Token.GetCollection(ci.ProjectCollectionUri))
                {
                    logger.Connecting(ci);
                    tfs.Authenticate();
                    var workItemStore = tfs.GetService<WorkItemStore>();
                    var qr = new QueryRunner(workItemStore, this.TeamProjectName);
                    var result = qr.RunQuery(this.WorkItemQuery);
                    if (result == null)
                    {
                        logger.QueryNotFound(this.WorkItemQuery, this.TeamProjectName);
                    }
                    else
                    {
                        foreach (var pair in result.WorkItems)
                        {
                            workItemIds.Enqueue(pair.Key);
                        }
                    }
                }
            }

            using (EventProcessor eventProcessor = new EventProcessor(runtime))
            {
                try
                {
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

                    return result?.StatusCode ?? -1;
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

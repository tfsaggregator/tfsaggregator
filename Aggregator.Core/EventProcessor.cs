namespace Aggregator.Core
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Aggregator.Core.Configuration;
    using Aggregator.Core.Facade;

    using Microsoft.VisualStudio.Services.Identity;

    using IdentityDescriptor = Microsoft.TeamFoundation.Framework.Client.IdentityDescriptor;

    /// <summary>
    /// This is the core class with complete logic, independent from being a server plug-in.
    /// It is the entry point of the Core assembly to manage a single request/event
    /// </summary>
    public class EventProcessor
    {
        TFSAggregatorSettings settings;
        ILogEvents logger;
        IWorkItemRepository store;
        ScriptEngine engine;

        // server ctor
        public EventProcessor(string tfsCollectionUrl, IdentityDescriptor toImpersonate, IRuntimeContext runtime)
            : this(new WorkItemRepository(tfsCollectionUrl, toImpersonate, runtime.Logger), runtime)
        {
        }

        // common ctor
        public EventProcessor(IWorkItemRepository workItemStore, IRuntimeContext runtime)
        {
            this.logger = runtime.Logger;
            this.store = workItemStore;
            this.settings = runtime.Settings;

            this.engine = runtime.GetEngine(workItemStore);                
        }

        /// <summary>
        /// This is the one where all the magic happens.
        /// </summary>
        public ProcessingResult ProcessEvent(IRequestContext requestContext, INotification notification)
        {
            var result = new ProcessingResult();

            IEnumerable<Policy> policies = this.FilterPolicies(this.settings.Policies, requestContext, notification);

            if (policies.Any())
            {
                IWorkItem workItem = this.store.GetWorkItem(notification.WorkItemId);

                foreach (var policy in policies)
                {
                    logger.ApplyingPolicy(policy.Name);
                    this.ApplyRules(workItem, policy.Rules);
                }

                this.SaveChangedWorkItems();
                result.StatusCode = 0;
                result.StatusMessage = "Success";
            }
            else
            {
                result.StatusCode = 1;
                result.StatusMessage = "No operation";
            }
            return result;
        }

        private IEnumerable<Policy> FilterPolicies(IEnumerable<Policy> policies, IRequestContext requestContext, INotification notification)
        {
            return policies.Where(policy => policy.Scope.All(s => s.Matches(requestContext, notification)));
        }

        private void ApplyRules(IWorkItem workItem, IEnumerable<Rule> rules)
        {
            foreach (var rule in rules)
            {
                this.logger.ApplyingRule(rule.Name);
                this.ApplyRule(rule, workItem);
            }
        }

        private void ApplyRule(Rule rule, IWorkItem workItem)
        {
            if (rule.Scope.All(s => s.Matches(workItem)))
            {
                this.logger.RunningRule(rule.Name, workItem);
                this.engine.Run(rule.Name, workItem);
            }
        }

        private void SaveChangedWorkItems()
        {
            // Save any changes to the target work items.
            foreach (IWorkItem workItem in this.store.LoadedWorkItems.Where(w => w.IsDirty))
            {
                bool isValid = workItem.IsValid();
                this.logger.Saving(workItem, isValid);
                if (isValid)
                {
                    workItem.PartialOpen();
                    workItem.Save();
                }
            }//for
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;

using Aggregator.Core.Configuration;
using Aggregator.Core.Context;
using Aggregator.Core.Facade;
using Aggregator.Core.Interfaces;
using Aggregator.Core.Monitoring;

using IdentityDescriptor = Microsoft.TeamFoundation.Framework.Client.IdentityDescriptor;

namespace Aggregator.Core
{
    /// <summary>
    /// This is the core class with complete logic, independent from being a server plug-in.
    /// It is the entry point of the Core assembly to manage a single request/event
    /// </summary>
    public class EventProcessor : IDisposable
    {
        private readonly TFSAggregatorSettings settings;

        private readonly ILogEvents logger;

        private readonly IWorkItemRepository store;

        private readonly ScriptEngine engine;

        /// <summary>
        /// Initializes a new instance of the <see cref="EventProcessor"/> class.
        /// </summary>
        /// <param name="tfsCollectionUrl">The TFS Project Colection Uri</param>
        /// <param name="toImpersonate">The IdentityDescriptor to Impoersonate</param>
        /// <param name="runtime">The runtime context</param>
        public EventProcessor(string tfsCollectionUrl, IdentityDescriptor toImpersonate, IRuntimeContext runtime)
            : this(new WorkItemRepository(tfsCollectionUrl, toImpersonate, runtime.Logger), runtime)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EventProcessor"/> class.
        /// </summary>
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
        /// <returns>The outcome of the policy Execution as per ISubscriber's contract</returns>
        /// <param name="requestContext">TFS Request Context</param>
        /// <param name="notification">The <paramref name="notification"/> containing the WorkItemChangedEvent</param>
        public ProcessingResult ProcessEvent(IRequestContext requestContext, INotification notification)
        {
            var result = new ProcessingResult();

            Policy[] policies = this.FilterPolicies(this.settings.Policies, requestContext, notification).ToArray();

            if (policies.Any())
            {
                IWorkItem workItem = this.store.GetWorkItem(notification.WorkItemId);

                foreach (var policy in policies)
                {
                    this.logger.ApplyingPolicy(policy.Name);
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
            // Save new work items to the target work items.
            foreach (IWorkItem workItem in this.store.CreatedWorkItems.Where(w => w.IsDirty))
            {
                this.ValidateAndSaveWorkItem(workItem);
            }

            // Save any changes to the target work items.
            foreach (IWorkItem workItem in this.store.LoadedWorkItems.Where(w => w.IsDirty))
            {
                this.ValidateAndSaveWorkItem(workItem);
            }
        }

        private void ValidateAndSaveWorkItem(IWorkItem workItem)
        {
            if (workItem.IsDirty)
            {
                bool isValid = workItem.IsValid();
                this.logger.Saving(workItem, isValid);

                if (isValid)
                {
                    workItem.PartialOpen();
                    workItem.Save();
                }
            }
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected void Dispose(bool disposing)
        {
            if (disposing)
            {
                (this.store as IDisposable)?.Dispose();
            }
        }
    }
}

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

        private readonly RateLimiter limiter;

        /// <summary>
        /// Initializes a new instance of the <see cref="EventProcessor"/> class.
        /// </summary>
        public EventProcessor(IRuntimeContext runtime)
        {
            this.logger = runtime.Logger;
            this.settings = runtime.Settings;
            this.limiter = runtime.RateLimiter;

            this.store = runtime.WorkItemRepository;
            this.engine = runtime.GetEngine();
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
                    this.ApplyRules(workItem, notification, policy.Rules);
                }

                this.SaveChangedWorkItems();
                result.StatusCode = 0;
                result.StatusMessage = "Success";
            }
            else
            {
                this.logger.NoPolicesApply();
                result.StatusCode = 1;
                result.StatusMessage = "No operation";
            }

            return result;
        }

        private IEnumerable<Policy> FilterPolicies(IEnumerable<Policy> policies, IRequestContext requestContext, INotification notification)
        {
            return policies.Where(policy => policy.Scope.All(scope =>
            {
                var result = scope.Matches(requestContext, notification);
                this.logger.PolicyScopeMatchResult(scope, result);
                return result.Success;
            }));
        }

        private void ApplyRules(IWorkItem workItem, INotification notification, IEnumerable<Rule> rules)
        {
            foreach (var rule in rules)
            {
                this.logger.ApplyingRule(rule.Name);
                this.ApplyRule(rule, workItem, notification);
            }
        }

        private void ApplyRule(Rule rule, IWorkItem workItem, INotification notification)
        {
            if (rule.Scope.All(scope =>
            {
                var result = scope.Matches(workItem, notification);
                this.logger.RuleScopeMatchResult(scope, result);
                return result.Success;
            }))
            {
                this.logger.RunningRule(rule.Name, workItem);
                this.engine.Run(rule.Name, workItem, this.store);
            }
        }

        private readonly List<int> savedWorkItemIds = new List<int>();

        public IEnumerable<int> SavedWorkItems
        {
            get
            {
                return this.savedWorkItemIds.AsEnumerable();
            }
        }

        private void SaveChangedWorkItems()
        {
            this.savedWorkItemIds.Clear();

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
                bool shouldLimit = workItem.ShouldLimit(this.limiter);

                this.logger.Saving(workItem, isValid, shouldLimit);

                if (isValid && !shouldLimit)
                {
                    if (this.settings.WhatIf)
                    {
                        this.logger.WhatIfSave(workItem);
                        // HACK
                        var wrapper = (WorkItemWrapper)workItem;
                        wrapper.RevertChanges();
                    }
                    else
                    {
                        workItem.PartialOpen();
                        workItem.Save();
                    }

                    // track
                    this.savedWorkItemIds.Add(workItem.Id);
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

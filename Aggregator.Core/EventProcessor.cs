using Aggregator.Core;
using Aggregator.Core.Configuration;
using Aggregator.Core.Facade;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aggregator.Core
{
    /// <summary>
    /// This is the core class with complete logic, independent from being a server plug-in
    /// </summary>
    public class EventProcessor
    {
        ILogEvents logger;
        IWorkItemRepository store;

        public EventProcessor(string tfsCollectionUrl, ILogEvents logger)
            : this(new WorkItemRepository(tfsCollectionUrl, logger), logger)
        {
        }

        public EventProcessor(IWorkItemRepository workItemStore, ILogEvents logger)
        {
            this.logger = logger;
            this.store = workItemStore;
        }

        /// <summary>
        /// This is the one where all the magic happens.
        /// </summary>
        public ProcessingResult ProcessEvent(IRequestContext requestContext, INotification notification, TFSAggregatorSettings settings)
        {
            // TODO
            var result = new ProcessingResult();

            Policy policy = FindApplicablePolicy(settings.Policies, requestContext, notification);
            if (policy != null)
            {
                IWorkItem workItem = store.GetWorkItem(notification.WorkItemId);
                ApplyRules(workItem, policy.Rules);
            }//if

            return result;
        }

        private Policy FindApplicablePolicy(IEnumerable<Policy> policies, IRequestContext requestContext, INotification notification)
        {
            foreach (var policy in policies)
            {
                if (policy.Scope.Matches(requestContext, notification))
                    return policy;
            }
            return null;
        }

        private void ApplyRules(IWorkItem workItem, IEnumerable<Rule> rules)
        {
            foreach (var rule in rules)
            {
                if (rule.ApplicableTypes.Contains(workItem.TypeName))
                    ApplyRule(rule, workItem);
            }
        }

        private void ApplyRule(Rule rule, IWorkItem workItem)
        {
            if (rule.ApplicableTypes.Contains(workItem.TypeName))
            {
                var engine = new CsScriptEngine(rule.Name, rule.Script, this.store, this.logger);
                engine.Run(workItem);
                // TODO to save all workitems we must trace them in WorkItemLazyReference... we need a queue in the RequestContext 
                workItem.Save();
            }
        }
    }
}

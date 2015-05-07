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
    using Microsoft.TeamFoundation.Framework.Client;

    /// <summary>
    /// This is the core class with complete logic, independent from being a server plug-in.
    /// It is the entry point of the Core assembly.
    /// </summary>
    public class EventProcessor
    {
        TFSAggregatorSettings settings;
        ILogEvents logger;
        IWorkItemRepository store;
        ScriptEngine engine;

        public EventProcessor(string tfsCollectionUrl, IdentityDescriptor toImpersonate, ILogEvents logger, TFSAggregatorSettings settings)
            : this(new WorkItemRepository(tfsCollectionUrl, toImpersonate, logger), logger, settings)
        {
        }

        public EventProcessor(IWorkItemRepository workItemStore, ILogEvents logger, TFSAggregatorSettings settings)
        {
            this.logger = logger;
            this.store = workItemStore;
            // TODO caching
            this.settings = settings;
            engine = MakeEngine(settings.ScriptLanguage, this.store, this.logger);
            foreach (var rule in settings.Rules)
            {
                engine.Load(rule.Name, rule.Script);
            }
            engine.LoadCompleted();
        }

        /// <summary>
        /// This is the one where all the magic happens.
        /// </summary>
        public ProcessingResult ProcessEvent(IRequestContext requestContext, INotification notification)
        {
            var result = new ProcessingResult();

            IEnumerable<Policy> policies = FilterPolicies(settings.Policies, requestContext, notification);

            if (policies.Any())
            {
                IWorkItem workItem = store.GetWorkItem(notification.WorkItemId);

                foreach (var policy in policies)
                {
                    logger.ApplyingPolicy(policy.Name);
                    ApplyRules(workItem, policy.Rules);
                }

                SaveChangedWorkItems();
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
                logger.ApplyingRule(rule.Name);
                ApplyRule(rule, workItem);
            }
        }

        private void ApplyRule(Rule rule, IWorkItem workItem)
        {
            if (rule.Scope.All(s => s.Matches(workItem)))
            {
                logger.RunningRule(rule.Name, workItem);
                engine.Run(rule.Name, workItem);
            }
        }

        private void SaveChangedWorkItems()
        {
            // Save any changes to the target work items.
            foreach (IWorkItem workItem in this.store.LoadedWorkItems.Where(w => w.IsDirty))
            {
                bool isValid = workItem.IsValid();
                logger.Saving(workItem, isValid);
                if (isValid)
                {
                    workItem.PartialOpen();
                    workItem.Save();
                }
            }//for
        }

        private ScriptEngine MakeEngine(string scriptLanguage, IWorkItemRepository workItemRepository, ILogEvents logEvents)
        {
            logger.BuildingScriptEngine(scriptLanguage);
            Type t = GetScriptEngineType(scriptLanguage);
            var ctor = t.GetConstructor(new Type[] { typeof(IWorkItemRepository), typeof(ILogEvents) });
            ScriptEngine engine = ctor.Invoke(new object[] { this.store, this.logger }) as ScriptEngine;
            return engine;
        }

        private Type GetScriptEngineType(string scriptLanguage)
        {
            switch (scriptLanguage.ToUpperInvariant())
            {
                case "CS":
                case "CSHARP":
                case "C#":
                    return typeof(CSharpScriptEngine);
                case "VB":
                case "VB.NET":
                case "VBNET":
                    return typeof(VBNetScriptEngine);
                case "PS":
                case "POWERSHELL":
                    return typeof(PsScriptEngine);
                default:
                    // TODO Log unsupported or wrong code
                    return typeof(CSharpScriptEngine);
            }
        }
    }
}

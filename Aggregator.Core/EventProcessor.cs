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
        TFSAggregatorSettings settings;
        ILogEvents logger;
        IWorkItemRepository store;
        ScriptEngine engine;

        public EventProcessor(string tfsCollectionUrl, ILogEvents logger, TFSAggregatorSettings settings)
            : this(new WorkItemRepository(tfsCollectionUrl, logger), logger, settings)
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

            Policy policy = FindApplicablePolicy(settings.Policies, requestContext, notification);
            if (policy != null)
            {
                IWorkItem workItem = store.GetWorkItem(notification.WorkItemId);
                ApplyRules(workItem, policy.Rules);
            }//if

            SaveChangedWorkItems();

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
                engine.Run(rule.Name, workItem);
            }
        }

        private void SaveChangedWorkItems()
        {
            // Save any changes to the target work items.
            foreach (IWorkItem workItem in this.store.LoadedWorkItems)
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
            Type t = GetScriptEngineType(scriptLanguage);
            var ctor = t.GetConstructor(new Type[] { typeof(IWorkItemRepository), typeof(ILogEvents) });
            ScriptEngine engine = ctor.Invoke(new object[] { this.store, this.logger }) as ScriptEngine;
            return engine;
        }

        private Type GetScriptEngineType(string scriptLanguage)
        {
            switch (scriptLanguage.ToLowerInvariant())
            {
                case "cs":
                case "csharp":
                case "c#":
                    return typeof(CSharpScriptEngine);
                case "vb":
                case "vb.net":
                case "vbnet":
                    return typeof(VBNetScriptEngine);
                case "ps":
                case "powershell":
                    return typeof(PsScriptEngine);
                default:
                    // TODO Log unsupported or wrong code
                    return typeof(CSharpScriptEngine);
            }
        }
    }
}

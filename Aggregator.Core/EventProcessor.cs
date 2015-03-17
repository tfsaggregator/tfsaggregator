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
                ApplyRules(workItem, policy.Rules, settings.ScriptLanguage);
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

        private void ApplyRules(IWorkItem workItem, IEnumerable<Rule> rules, string scriptLanguage)
        {
            foreach (var rule in rules)
            {
                if (rule.ApplicableTypes.Contains(workItem.TypeName))
                    ApplyRule(rule, workItem, scriptLanguage);
            }
        }

        private void ApplyRule(Rule rule, IWorkItem workItem, string scriptLanguage)
        {
            if (rule.ApplicableTypes.Contains(workItem.TypeName))
            {
                ScriptEngine engine = MakeEngine(scriptLanguage, rule.Name, rule.Script, this.store, this.logger);
                engine.Run(workItem);
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

        private ScriptEngine MakeEngine(string scriptLanguage, string ruleName, string script, IWorkItemRepository workItemRepository, ILogEvents logEvents)
        {
            Type t = GetScriptEngineType(scriptLanguage);
            var ctor = t.GetConstructor(new Type[]{typeof(string),typeof(string),typeof(IWorkItemRepository),typeof(ILogEvents)});
            ScriptEngine engine = ctor.Invoke(new object[] { ruleName, script, this.store, this.logger }) as ScriptEngine;
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
                    return typeof(CSharpScriptEngine);
            }
        }
    }
}

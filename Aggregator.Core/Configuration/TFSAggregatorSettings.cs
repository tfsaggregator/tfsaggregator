using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace Aggregator.Core.Configuration
{
    public class TFSAggregatorSettings
    {
        public IEnumerable<Rule> Rules { get; set; }
        public IEnumerable<Policy> Policies { get; set; }

        public static TFSAggregatorSettings LoadFromFile(string settingsPath)
        {
            throw new NotImplementedException();
        }

        public static TFSAggregatorSettings LoadXml(string content)
        {
            var instance = new TFSAggregatorSettings();

            LoadOptions xmlLoadOptions = LoadOptions.PreserveWhitespace | LoadOptions.SetBaseUri | LoadOptions.SetLineInfo;
            XDocument doc = XDocument.Parse(content, xmlLoadOptions);

            LogLevel logLevel;
            if (Enum.TryParse<LogLevel>(doc.Root.Attribute("logLevel").Value, out logLevel))
                instance.LogLevel = logLevel;

            var rules = new Dictionary<string, Rule>();
            foreach (var ruleElem in doc.Root.Elements("rule"))
            {
                var rule = new Rule()
                {
                    Name = ruleElem.Attribute("name").Value,
                };

                var applicableTypes = new List<string>();
                applicableTypes.AddRange(ruleElem.Attribute("appliesTo").Value.Split(',', ';'));
                rule.ApplicableTypes = applicableTypes;
                rule.Script = ruleElem.Value;

                rules.Add(rule.Name, rule);
            }//for
            instance.Rules = rules.Values.ToList();

            var policies = new List<Policy>();
            foreach (var policyElem in doc.Root.Elements("policy"))
            {
                var policy = new Policy()
                {
                    Name = policyElem.Attribute("name").Value,
                };
                //TODO fails for other scope types
                var scopeElem = policyElem.Element("collectionScope");
                var collections = new List<string>();
                collections.AddRange(scopeElem.Attribute("collections").Value.Split(',', ';'));
                policy.Scope = new CollectionScope()
                {
                    CollectionNames = collections
                };
                var referredRules = new List<Rule>();
                foreach (var ruleRefElem in policyElem.Elements("ruleRef"))
                {
                    string refName = ruleRefElem.Attribute("name").Value;
                    var rule = rules[refName];
                    referredRules.Add(rule);
                }
                policy.Rules = referredRules;
                
                policies.Add(policy);
            }//for
            instance.Policies = policies;

            return instance;
        }

        public LogLevel LogLevel { get; set; }
    }
}

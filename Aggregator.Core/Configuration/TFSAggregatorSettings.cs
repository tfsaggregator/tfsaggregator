using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace Aggregator.Core.Configuration
{
    /// <summary>
    /// This class' represents Core settings as properties
    /// </summary>
    public class TFSAggregatorSettings
    {
        public IEnumerable<Rule> Rules { get; set; }
        public IEnumerable<Policy> Policies { get; set; }

        public static TFSAggregatorSettings LoadFromFile(string settingsPath)
        {
            return Load((xmlLoadOptions) => XDocument.Load(settingsPath, xmlLoadOptions));
        }

        public static TFSAggregatorSettings LoadXml(string content)
        {
            return Load((xmlLoadOptions) => XDocument.Parse(content, xmlLoadOptions));
        }

        /// <summary>
        /// Parse the specified <see cref="XDocument"/> to build a <see cref="TFSAggregatorSettings"/> instance.
        /// </summary>
        /// <param name="load">A lambda returning the <see cref="XDocument"/> to parse.</param>
        /// <returns></returns>
        public static TFSAggregatorSettings Load(Func<LoadOptions, XDocument> load)
        {
            var instance = new TFSAggregatorSettings();

            LoadOptions xmlLoadOptions = LoadOptions.PreserveWhitespace | LoadOptions.SetBaseUri | LoadOptions.SetLineInfo;
            XDocument doc = load(xmlLoadOptions);

            LogLevel logLevel;
            if (Enum.TryParse<LogLevel>(doc.Root.Attribute("logLevel").Value, out logLevel))
                instance.LogLevel = logLevel;

            var scriptLangAttribute = doc.Root.Attribute("scriptLanguage");
            instance.ScriptLanguage = scriptLangAttribute != null ? scriptLangAttribute.Value : "CSharp";

            var rules = new Dictionary<string, Rule>();
            foreach (var ruleElem in doc.Root.Elements("rule"))
            {
                var rule = new Rule()
                {
                    Name = ruleElem.Attribute("name").Value,
                };

                var ruleScopes = new List<RuleScope>();

                if (ruleElem.Attribute("appliesTo") != null)
                {
                    var applicableTypes = new List<string>();
                    applicableTypes.AddRange(ruleElem.Attribute("appliesTo").Value.Split(',', ';'));
                    ruleScopes.Add( new WorkItemTypeScope() { ApplicableTypes = applicableTypes.ToArray() });
                }

                if (ruleElem.Attribute("hasFields") != null)
                {
                    var hasFields = new List<string>();
                    hasFields.AddRange(ruleElem.Attribute("hasFields").Value.Split(',', ';'));
                    ruleScopes.Add( new HasFieldsScope() { FieldNames = hasFields.ToArray() });
                }

                rule.Scope = ruleScopes.ToArray();
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

                List<PolicyScope>  scope = new List<PolicyScope>();
                var nullAttribute = new XAttribute("empty", string.Empty);

                foreach (var element in doc.Root.Elements())
                {
                    switch (element.Name.LocalName)
                    {
                        case "collectionScope":
                        {
                            var collections = new List<string>();
                            collections.AddRange((element.Attribute("collections") ?? nullAttribute).Value.Split(',', ';'));
                            scope.Add(new CollectionScope() { CollectionNames = collections });
                            break;
                        }
                        case "templateScope":
                        {
                            string templateName = (element.Attribute("name")       ?? nullAttribute).Value;
                            string templateId =   (element.Attribute("typeId")     ?? nullAttribute).Value;
                            string minVersion =   (element.Attribute("minVersion") ?? nullAttribute).Value;
                            string maxVersion =   (element.Attribute("maxValue")   ?? nullAttribute).Value;

                            scope.Add(new TemplateScope()
                            {
                                TemplateName = templateName,
                                TemplateTypeId = templateId,
                                MinVersion = minVersion,
                                MaxVersion = maxVersion
                            });            
                            break;
                        }
                        case "projectScope":
                        {
                            var projects = new List<string>();
                            projects.AddRange((element.Attribute("projects") ?? nullAttribute).Value.Split(',', ';'));
                            scope.Add(new ProjectScope() { ProjectNames = projects });
                            break;
                        }
                    }
                }

                policy.Scope = scope;

                var referredRules = new List<Rule>();
                foreach (var ruleRefElem in policyElem.Elements("ruleRef"))
                {
                    string refName = ruleRefElem.Attribute("name").Value;
                    var rule = rules[refName];
                    referredRules.Add(rule);
                }
                policy.Rules = referredRules;
                
                policies.Add(policy);
            }

            instance.Policies = policies;

            return instance;
        }

        public LogLevel LogLevel { get; set; }
        public string ScriptLanguage { get; set; }
    }
}

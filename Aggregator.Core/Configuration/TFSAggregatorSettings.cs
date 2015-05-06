using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml.Linq;
using System.Xml.Schema;

namespace Aggregator.Core.Configuration
{
    /// <summary>
    /// This class' represents Core settings as properties
    /// </summary>
    public class TFSAggregatorSettings
    {
        static readonly char[] ListSeparators = new char[] { ',', ';' };

        public IEnumerable<Rule> Rules { get; set; }
        public IEnumerable<Policy> Policies { get; set; }

        public static TFSAggregatorSettings LoadFromFile(string settingsPath, ILogEvents logger)
        {
            return Load(logger, (xmlLoadOptions) => XDocument.Load(settingsPath, xmlLoadOptions));
        }

        public static TFSAggregatorSettings LoadXml(string content, ILogEvents logger)
        {
            return Load(logger, (xmlLoadOptions) => XDocument.Parse(content, xmlLoadOptions));
        }

        /// <summary>
        /// Parse the specified <see cref="XDocument"/> to build a <see cref="TFSAggregatorSettings"/> instance.
        /// </summary>
        /// <param name="load">A lambda returning the <see cref="XDocument"/> to parse.</param>
        /// <returns></returns>
        public static TFSAggregatorSettings Load(ILogEvents logger, Func<LoadOptions, XDocument> load)
        {
            var instance = new TFSAggregatorSettings();

            LoadOptions xmlLoadOptions = LoadOptions.PreserveWhitespace | LoadOptions.SetBaseUri | LoadOptions.SetLineInfo;
            XDocument doc = load(xmlLoadOptions);

            XmlSchemaSet schemas = new XmlSchemaSet();
            var thisAssembly = Assembly.GetAssembly(typeof(TFSAggregatorSettings));
            var stream = thisAssembly.GetManifestResourceStream("Aggregator.Core.Configuration.AggregatorConfiguration.xsd");
            schemas.Add("", System.Xml.XmlReader.Create(stream));
            bool errors = false;
            doc.Validate(schemas, (o, e) =>
            {
                logger.InvalidConfiguration(e.Severity, e.Message, e.Exception.LineNumber, e.Exception.LinePosition);
                errors = true;
            }, true);
            if (errors)
                // HACK we must handle this scenario with clean exit
                return null;

            // XML Schema has done lot of checking and set defaults, no need to recheck here
            instance.LogLevel = (LogLevel)Enum.Parse(typeof(LogLevel), doc.Root.Attribute("logLevel").Value);
            instance.AutoImpersonate = bool.Parse(doc.Root.Attribute("autoImpersonate").Value);
            instance.ScriptLanguage = doc.Root.Attribute("scriptLanguage").Value;

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
                    ruleScopes.Add( new WorkItemTypeScope() { ApplicableTypes = ruleElem.Attribute("appliesTo").Value.Split(ListSeparators) });
                }

                if (ruleElem.Attribute("hasFields") != null)
                {
                    ruleScopes.Add( new HasFieldsScope() { FieldNames = ruleElem.Attribute("hasFields").Value.Split(ListSeparators) });
                }

                rule.Scope = ruleScopes.ToArray();
                rule.Script = ruleElem.Value;

                rules.Add(rule.Name, rule);
            }//for rule
            instance.Rules = rules.Values.ToList();

            var ruleInUse = new Dictionary<string, bool>();
            foreach (string ruleName in rules.Keys)
            {
                ruleInUse.Add(ruleName, false);
            }//for

            var policies = new List<Policy>();
            foreach (var policyElem in doc.Root.Elements("policy"))
            {
                var policy = new Policy()
                {
                    Name = policyElem.Attribute("name").Value,
                };

                List<PolicyScope>  scope = new List<PolicyScope>();
                var nullAttribute = new XAttribute("empty", string.Empty);

                foreach (var element in policyElem.Elements())
                {
                    switch (element.Name.LocalName)
                    {
                        case "collectionScope":
                        {
                            var collections = new List<string>();
                            collections.AddRange((element.Attribute("collections") ?? nullAttribute).Value.Split(ListSeparators));
                            scope.Add(new CollectionScope() { CollectionNames = collections });
                            break;
                        }//case
                        case "templateScope":
                        {
                            string templateName = (element.Attribute("name")       ?? nullAttribute).Value;
                            string templateId =   (element.Attribute("typeId")     ?? nullAttribute).Value;
                            string minVersion =   (element.Attribute("minVersion") ?? nullAttribute).Value;
                            string maxVersion =   (element.Attribute("maxVersion")   ?? nullAttribute).Value;

                            scope.Add(new TemplateScope()
                            {
                                TemplateName = templateName,
                                TemplateTypeId = templateId,
                                MinVersion = minVersion,
                                MaxVersion = maxVersion
                            });            
                            break;
                        }//case
                        case "projectScope":
                        {
                            var projects = new List<string>();
                            projects.AddRange((element.Attribute("projects") ?? nullAttribute).Value.Split(ListSeparators));
                            scope.Add(new ProjectScope() { ProjectNames = projects });
                            break;
                        }//case
                    }//switch
                }//for

                policy.Scope = scope;

                var referredRules = new List<Rule>();
                foreach (var ruleRefElem in policyElem.Elements("ruleRef"))
                {
                    string refName = ruleRefElem.Attribute("name").Value;
                    var rule = rules[refName];
                    referredRules.Add(rule);

                    ruleInUse[refName] = true;
                }
                policy.Rules = referredRules;
                
                policies.Add(policy);
            }//for policy

            instance.Policies = policies;

            // checks
            foreach (var unusedRule in ruleInUse.Where(kv => kv.Value == false))
            {
                logger.UnreferencedRule(unusedRule.Key);
            }//for

            return instance;
        }

        public LogLevel LogLevel { get; set; }
        public string ScriptLanguage { get; set; }
        public bool AutoImpersonate { get; set; }
    }
}

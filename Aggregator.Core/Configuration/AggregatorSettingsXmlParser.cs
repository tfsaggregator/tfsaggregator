using Aggregator.Core.Monitoring;

namespace Aggregator.Core.Configuration
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Xml;
    using System.Xml.Linq;
    using System.Xml.Schema;

    public partial class TFSAggregatorSettings
    {
        public class AggregatorSettingsXmlParser
        {
            private TFSAggregatorSettings instance;
            private readonly ILogEvents logger;

            public AggregatorSettingsXmlParser(ILogEvents logger)
            {
                this.logger = logger;
            }

            public TFSAggregatorSettings Parse(DateTime lastWriteTime, Func<LoadOptions, XDocument> load)
            {
                this.instance = new TFSAggregatorSettings();

                LoadOptions xmlLoadOptions = LoadOptions.PreserveWhitespace | LoadOptions.SetBaseUri | LoadOptions.SetLineInfo;
                XDocument doc = load(xmlLoadOptions);

                this.instance.Hash = this.ComputeHash(doc, lastWriteTime);

                if (!this.ValidateDocAgainstSchema(doc))
                {
                    // HACK we must handle this scenario with clean exit
                    return null;
                }

                // XML Schema has done lot of checking and set defaults, no need to recheck later, just manage missing pieces
                this.ParseRuntimeSection(doc);

                Dictionary<string, Rule> rules = this.ParseRulesSection(doc);

                var ruleInUse = rules.Keys.ToDictionary(ruleName => ruleName, ruleName => false);

                List<Policy> policies = this.ParsePoliciesSection(doc, rules, ruleInUse);

                this.instance.Policies = policies;

                // checks
                foreach (var unusedRule in ruleInUse.Where(kv => kv.Value == false))
                {
                    this.logger.UnreferencedRule(unusedRule.Key);
                }

                return this.instance;
            }

            private string ComputeHash(XDocument doc, DateTime timestamp)
            {
                using (var stream = new System.IO.MemoryStream())
                using (var md5 = new System.Security.Cryptography.MD5CryptoServiceProvider())
                using (var w = new System.IO.BinaryWriter(stream))
                {
                    w.Write(timestamp.ToBinary());
                    w.Flush();
                    doc.Save(stream, SaveOptions.OmitDuplicateNamespaces);
                    stream.Flush();
                    var hash = md5.ComputeHash(stream.GetBuffer());
                    string hex = BitConverter.ToString(hash);
                    return hex.Replace("-", string.Empty);
                }
            }

            private bool ValidateDocAgainstSchema(XDocument doc)
            {
                XmlSchemaSet schemas = new XmlSchemaSet();
                var thisAssembly = Assembly.GetAssembly(typeof(TFSAggregatorSettings));
                var stream = thisAssembly.GetManifestResourceStream("Aggregator.Core.Configuration.AggregatorConfiguration.xsd");
                schemas.Add(string.Empty, XmlReader.Create(stream));
                bool valid = true;
                doc.Validate(schemas, (o, e) =>
                {
                    this.logger.InvalidConfiguration(e.Severity, e.Message, e.Exception.LineNumber, e.Exception.LinePosition);
                    valid = false;
                }, true);
                return valid;
            }

            private void ParseRuntimeSection(XDocument doc)
            {
                var loggingNode = doc.Root.Element("runtime") != null ?
                    doc.Root.Element("runtime")?.Element("logging") : null;
                this.instance.LogLevel = loggingNode != null ?
                    (LogLevel)Enum.Parse(typeof(LogLevel), loggingNode.Attribute("level").Value)
                    : LogLevel.Normal;

                var rateLimitElement = doc.Root.Element("runtime")?.Element("rateLimiting");
                if (rateLimitElement != null)
                {
                    var rateLimit = new RateLimit();
                    rateLimit.Changes = int.Parse(rateLimitElement.Attribute("changes").Value);
                    rateLimit.Interval = TimeSpan.Parse(rateLimitElement.Attribute("interval").Value);
                    this.instance.RateLimit = rateLimit;
                }
                else
                {
                    this.instance.RateLimit = null;
                }

                var runtimeNode = doc.Root.Element("runtime") ?? null;
                var debugvalue = runtimeNode?.Attribute("debug")?.Value;
                this.instance.Debug = debugvalue != null && bool.Parse(debugvalue);

                var authenticationNode = doc.Root.Element("runtime") != null ?
                    doc.Root.Element("runtime")?.Element("authentication") : null;
                this.instance.AutoImpersonate = authenticationNode != null
                    && bool.Parse(authenticationNode.Attribute("autoImpersonate").Value);

                var scriptNode = doc.Root.Element("runtime") != null ?
                    doc.Root.Element("runtime")?.Element("script") : null;

                this.instance.ScriptLanguage = scriptNode?.Attribute("language").Value ?? "C#";
            }

            private List<Policy> ParsePoliciesSection(XDocument doc, Dictionary<string, Rule> rules, Dictionary<string, bool> ruleInUse)
            {
                var policies = new List<Policy>();
                foreach (var policyElem in doc.Root.Elements("policy"))
                {
                    var policy = new Policy()
                    {
                        Name = policyElem.Attribute("name").Value,
                    };

                    List<PolicyScope> scope = new List<PolicyScope>();
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
                                }

                            case "templateScope":
                                {
                                    // TODO check for proper attribute combo (cannot be done in XSD)
                                    string templateName = (element.Attribute("name") ?? nullAttribute).Value;
                                    string templateId = (element.Attribute("typeId") ?? nullAttribute).Value;
                                    string minVersion = (element.Attribute("minVersion") ?? nullAttribute).Value;
                                    string maxVersion = (element.Attribute("maxVersion") ?? nullAttribute).Value;

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
                                    projects.AddRange((element.Attribute("projects") ?? nullAttribute).Value.Split(ListSeparators));
                                    scope.Add(new ProjectScope() { ProjectNames = projects });
                                    break;
                                }

                            default:
                                {
                                    // Ignore other rules for now.
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

                        ruleInUse[refName] = true;
                    }

                    policy.Rules = referredRules;

                    policies.Add(policy);
                }

                return policies;
            }

            private Dictionary<string, Rule> ParseRulesSection(XDocument doc)
            {
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
                        ruleScopes.Add(new WorkItemTypeScope() { ApplicableTypes = ruleElem.Attribute("appliesTo").Value.Split(ListSeparators) });
                    }

                    if (ruleElem.Attribute("hasFields") != null)
                    {
                        ruleScopes.Add(new HasFieldsScope() { FieldNames = ruleElem.Attribute("hasFields").Value.Split(ListSeparators) });
                    }

                    rule.Scope = ruleScopes.ToArray();
                    rule.Script = ruleElem.Value;

                    rules.Add(rule.Name, rule);
                }

                this.instance.Rules = rules.Values.ToList();
                return rules;
            }
        }
    }
}

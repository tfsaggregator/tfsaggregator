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

#pragma warning disable S101 // Types should be named in camel case
    public partial class TFSAggregatorSettings
#pragma warning restore S101 // Types should be named in camel case
    {
        /// <summary>
        /// Populates a <see cref="TFSAggregatorSettings"/> instance parsing an <see cref="XDocument" />.
        /// </summary>
        /// <remarks>As a nested class can access all private setters of <see cref="TFSAggregatorSettings"/></remarks>
        public class AggregatorSettingsXmlParser
        {
            private TFSAggregatorSettings instance;
            private readonly ILogEvents logger;

            public AggregatorSettingsXmlParser(ILogEvents logger)
            {
                this.logger = logger;
            }

            /// <summary>
            /// Parse the specified <see cref="XDocument"/> to build a <see cref="TFSAggregatorSettings"/> instance.
            /// </summary>
            /// <param name="lastWriteTime">Last time the document has been changed.</param>
            /// <param name="load">A lambda returning the <see cref="XDocument"/> to parse.</param>
            /// <returns>An instance of <see cref="TFSAggregatorSettings"/> or null</returns>
            public TFSAggregatorSettings Parse(DateTime lastWriteTime, Func<LoadOptions, XDocument> load)
            {
                this.instance = new TFSAggregatorSettings();

                LoadOptions xmlLoadOptions = LoadOptions.PreserveWhitespace | LoadOptions.SetBaseUri | LoadOptions.SetLineInfo;
                XDocument doc = load(xmlLoadOptions);

                this.instance.Hash = this.ComputeHash(doc, lastWriteTime);

                if (!this.ValidateDocAgainstSchema(doc))
                {
                    return null;
                }

                // XML Schema has done lot of checking and set defaults, no need to recheck later, just manage missing pieces
                this.ParseRuntimeSection(doc);

                this.instance.Snippets = this.ParseSnippetsSection(doc);
                this.instance.Functions = this.ParseFunctionsSection(doc);

                Dictionary<string, Rule> rules = this.ParseRulesSection(doc);

                List<Policy> policies = this.ParsePoliciesSection(doc, rules);

                this.instance.Policies = policies;

                this.ValidateSemantic(rules);

                return this.instance;
            }

            private void ValidateSemantic(Dictionary<string, Rule> rules)
            {
                foreach (var policy in this.instance.Policies)
                {
                    if (!policy.Scope.Any())
                    {
                        this.logger.PolicyShouldHaveAScope(policy.Name);
                    }
                }

                var usedRules = new List<Rule>();
                foreach (var policy in this.instance.Policies)
                {
                    usedRules.AddRange(policy.Rules);
                }

                var unusedRules = rules.Values.Except(usedRules);

                // check if there Rule are referenced at least once
                foreach (var unusedRule in unusedRules)
                {
                    this.logger.UnreferencedRule(unusedRule.Name);
                }
            }

            /// <summary>
            /// Simple hash to detect configuration changes
            /// </summary>
            /// <param name="doc">Configuration XML document.</param>
            /// <param name="timestamp">Last time the document has been changed.</param>
            /// <returns>Hexadecimal string represting hash value</returns>
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
                doc.Validate(
                    schemas,
                    (o, e) =>
                    {
                        this.logger.InvalidConfiguration(e.Severity, e.Message, e.Exception.LineNumber, e.Exception.LinePosition);
                        valid = false;
                    },
                    true);
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
                    var rateLimit = new RateLimit()
                    {
                        Changes = int.Parse(rateLimitElement.Attribute("changes").Value),
                        Interval = TimeSpan.Parse(rateLimitElement.Attribute("interval").Value)
                    };
                    this.instance.RateLimit = rateLimit;
                }
                else
                {
                    this.instance.RateLimit = null;
                }

                var runtimeNode = doc.Root.Element("runtime") ?? null;
                var debugvalue = runtimeNode?.Attribute("debug")?.Value;
                this.instance.Debug = debugvalue != null && bool.Parse(debugvalue);
                var whatifvalue = runtimeNode?.Attribute("whatIf")?.Value;
                this.instance.WhatIf = whatifvalue != null && bool.Parse(whatifvalue);

                var authenticationNode = doc.Root.Element("runtime") != null ?
                    doc.Root.Element("runtime")?.Element("authentication") : null;
                this.instance.AutoImpersonate = authenticationNode != null
                    && bool.Parse(authenticationNode.Attribute("autoImpersonate").Value);
                this.instance.PersonalToken =
                    authenticationNode?.Attribute("personalToken")?.Value;
                this.instance.BasicUsername = authenticationNode?.Attribute("username")?.Value;
                this.instance.BasicPassword = authenticationNode?.Attribute("password")?.Value;

                var scriptNode = doc.Root.Element("runtime") != null ?
                    doc.Root.Element("runtime")?.Element("script") : null;

                this.instance.ScriptLanguage = scriptNode?.Attribute("language").Value ?? "C#";

                var serverNode = doc.Root.Element("runtime") != null ?
                    doc.Root.Element("runtime")?.Element("server") : null;
                string baseUrl = serverNode?.Attribute("baseUrl")?.Value;
                this.instance.ServerBaseUrl = string.IsNullOrWhiteSpace(baseUrl)
                    ? null
                    : new Uri(new Uri(baseUrl).GetLeftPart(UriPartial.Authority));
                this.instance.IgnoreSslErrors = serverNode != null
                    && bool.Parse(serverNode.Attribute("ignoreSslErrors")?.Value);
            }

            private List<Policy> ParsePoliciesSection(XDocument doc, Dictionary<string, Rule> rules)
            {
                var policies = new List<Policy>();
                foreach (var policyElem in doc.Root.Elements("policy"))
                {
                    var policy = new Policy()
                    {
                        Name = policyElem.Attribute("name").Value,
                    };

                    List<PolicyScope> scope = new List<PolicyScope>();

                    // trick to return string.Empty in case of missing attribute
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
                                    string templateName = (element.Attribute("name") ?? nullAttribute).Value;

                                    // check for proper attribute combo (cannot be done in XSD)
                                    if (string.IsNullOrWhiteSpace(templateName))
                                    {
                                        this.logger.TemplateScopeConfigurationRequiresAtLeastName();
                                    }
                                    else
                                    {
                                        scope.Add(new TemplateScope()
                                        {
                                            TemplateName = templateName
                                        });
                                    }

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
                    }

                    policy.Rules = referredRules;

                    policies.Add(policy);
                }

                return policies;
            }

            private List<Snippet> ParseSnippetsSection(XDocument doc)
            {
                var snippets = new List<Snippet>();
                foreach (var snippetElem in doc.Root.Elements("snippet"))
                {
                    var snippet = new Snippet()
                    {
                        Name = snippetElem.Attribute("name").Value,
                        Script = snippetElem.Value
                    };
                    snippets.Add(snippet);
                }

                return snippets;
            }

            private IList<Function> ParseFunctionsSection(XDocument doc)
            {
                var functions = new List<Function>();
                foreach (var functionElem in doc.Root.Elements("function"))
                {
                    var function = new Function()
                    {
                        Script = functionElem.Value
                    };

                    functions.Add(function);
                }

                return functions;
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

                    if (ruleElem.Attribute("changes") != null)
                    {
                        ruleScopes.Add(new ChangeTypeScope() { ApplicableChanges = ruleElem.Attribute("changes").Value.Split(ListSeparators) });
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

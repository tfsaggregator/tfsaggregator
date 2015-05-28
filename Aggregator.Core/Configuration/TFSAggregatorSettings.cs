namespace Aggregator.Core.Configuration
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Xml;
    using System.Xml.Linq;
    using System.Xml.Schema;

    /// <summary>
    /// This class' represents Core settings as properties
    /// </summary>
    public class TFSAggregatorSettings
    {
        static readonly char[] ListSeparators = new char[] { ',', ';' };

        public static TFSAggregatorSettings LoadFromFile(string settingsPath, ILogEvents logger)
        {
            DateTime timestamp = System.IO.File.GetLastWriteTimeUtc(settingsPath);
            return Load(timestamp, (xmlLoadOptions) => XDocument.Load(settingsPath, xmlLoadOptions), logger);
        }

        public static TFSAggregatorSettings LoadXml(string content, ILogEvents logger)
        {
            // conventional point in time reference
            DateTime timestamp = new DateTime(0, DateTimeKind.Utc);
            return LoadXml(content, timestamp, logger);
        }

        public static TFSAggregatorSettings LoadXml(string content, DateTime timestamp, ILogEvents logger)
        {
            return Load(timestamp, (xmlLoadOptions) => XDocument.Parse(content, xmlLoadOptions), logger);
        }

        /// <summary>
        /// Parse the specified <see cref="XDocument"/> to build a <see cref="TFSAggregatorSettings"/> instance.
        /// </summary>
        /// <param name="load">A lambda returning the <see cref="XDocument"/> to parse.</param>
        /// <returns></returns>
        public static TFSAggregatorSettings Load(DateTime timestamp, Func<LoadOptions, XDocument> load, ILogEvents logger)
        {
            var instance = new TFSAggregatorSettings();

            LoadOptions xmlLoadOptions = LoadOptions.PreserveWhitespace | LoadOptions.SetBaseUri | LoadOptions.SetLineInfo;
            XDocument doc = load(xmlLoadOptions);

            instance.Hash = ComputeHash(doc, timestamp);

            if (!ValidateDocAgainstSchema(doc, logger))
                // HACK we must handle this scenario with clean exit
                return null;

            // XML Schema has done lot of checking and set defaults, no need to recheck later, just manage missing pieces

            ParseRuntimeSection(instance, doc);

            Dictionary<string, Rule> rules = ParseRulesSection(instance, doc);

            var ruleInUse = new Dictionary<string, bool>();
            foreach (string ruleName in rules.Keys)
            {
                ruleInUse.Add(ruleName, false);
            }//for

            List<Policy> policies = ParsePoliciesSection(doc, rules, ruleInUse);

            instance.Policies = policies;

            // checks
            foreach (var unusedRule in ruleInUse.Where(kv => kv.Value == false))
            {
                logger.UnreferencedRule(unusedRule.Key);
            }//for

            return instance;
        }

        private static string ComputeHash(XDocument doc, DateTime timestamp)
        {
            using (var stream = new System.IO.MemoryStream())
            {
                using (var md5 = new System.Security.Cryptography.MD5CryptoServiceProvider())
                {
                    using (var w = new System.IO.BinaryWriter(stream))
                    {
                        w.Write(timestamp.ToBinary());
                        w.Flush();
                        doc.Save(stream, SaveOptions.OmitDuplicateNamespaces);
                        stream.Flush();
                        var hash = md5.ComputeHash(stream.GetBuffer());
                        string hex = BitConverter.ToString(hash);
                        return hex.Replace("-", "");
                    }
                }
            }
        }

        private static bool ValidateDocAgainstSchema(XDocument doc, ILogEvents logger)
        {
            XmlSchemaSet schemas = new XmlSchemaSet();
            var thisAssembly = Assembly.GetAssembly(typeof(TFSAggregatorSettings));
            var stream = thisAssembly.GetManifestResourceStream("Aggregator.Core.Configuration.AggregatorConfiguration.xsd");
            schemas.Add("", XmlReader.Create(stream));
            bool valid = true;
            doc.Validate(schemas, (o, e) =>
            {
                logger.InvalidConfiguration(e.Severity, e.Message, e.Exception.LineNumber, e.Exception.LinePosition);
                valid = false;
            }, true);
            return valid;
        }

        private static void ParseRuntimeSection(TFSAggregatorSettings instance, XDocument doc)
        {
            var loggingNode = doc.Root.Element("runtime") != null ?
                doc.Root.Element("runtime").Element("logging") : null;
            instance.LogLevel = loggingNode != null ?
                (LogLevel)Enum.Parse(typeof(LogLevel), loggingNode.Attribute("level").Value)
                : LogLevel.Normal;
            var authenticationNode = doc.Root.Element("runtime") != null ?
                doc.Root.Element("runtime").Element("authentication") : null;
            instance.AutoImpersonate = authenticationNode != null ?
                bool.Parse(authenticationNode.Attribute("autoImpersonate").Value)
                : false;
            var scriptNode = doc.Root.Element("runtime") != null ?
                doc.Root.Element("runtime").Element("script") : null;
            instance.ScriptLanguage = scriptNode != null ?
                scriptNode.Attribute("language").Value
                : "C#";
        }

        private static List<Policy> ParsePoliciesSection(XDocument doc, Dictionary<string, Rule> rules, Dictionary<string, bool> ruleInUse)
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
                            }//case
                        case "templateScope":
                            {
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

            return policies;
        }

        private static Dictionary<string, Rule> ParseRulesSection(TFSAggregatorSettings instance, XDocument doc)
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
            }//for rule
            instance.Rules = rules.Values.ToList();
            return rules;
        }

        public LogLevel LogLevel { get; private set; }
        public string ScriptLanguage { get; private set; }
        public bool AutoImpersonate { get; private set; }
        public string Hash { get; private set; }
        public IEnumerable<Rule> Rules { get; private set; }
        public IEnumerable<Policy> Policies { get; private set; }
    }
}

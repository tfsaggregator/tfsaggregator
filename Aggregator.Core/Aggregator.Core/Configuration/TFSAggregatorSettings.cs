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
    /// <summary>
    /// This class represents Core settings as properties
    /// </summary>
    /// <remarks>Marked partial to apply nested trick</remarks>
    public partial class TFSAggregatorSettings
#pragma warning restore S101 // Types should be named in camel case
    {
        private static readonly char[] ListSeparators = new char[] { ',', ';' };

        /// <summary>
        /// Load configuration from file. Main scenario.
        /// </summary>
        /// <param name="settingsPath">Path to policies file</param>
        /// <param name="logger">Logging object</param>
        /// <returns>An instance of <see cref="TFSAggregatorSettings"/> or null</returns>
        public static TFSAggregatorSettings LoadFromFile(string settingsPath, ILogEvents logger)
        {
            DateTime lastWriteTime
                = System.IO.File.GetLastWriteTimeUtc(settingsPath);
            var parser = new AggregatorSettingsXmlParser(logger);
            return parser.Parse(lastWriteTime, (xmlLoadOptions) => XDocument.Load(settingsPath, xmlLoadOptions));
        }

        /// <summary>
        /// Load configuration from string. Used by automated tests.
        /// </summary>
        /// <param name="content">Configuration data to parse</param>
        /// <param name="logger">Logging object</param>
        /// <returns>An instance of <see cref="TFSAggregatorSettings"/> or null</returns>
        public static TFSAggregatorSettings LoadXml(string content, ILogEvents logger)
        {
            // conventional point in time reference
            DateTime staticTimestamp = new DateTime(0, DateTimeKind.Utc);
            var parser = new AggregatorSettingsXmlParser(logger);
            return parser.Parse(staticTimestamp, (xmlLoadOptions) => XDocument.Parse(content, xmlLoadOptions));
        }

        public LogLevel LogLevel { get; private set; }

        public string ScriptLanguage { get; private set; }

        public bool AutoImpersonate { get; private set; }

        public Uri ServerBaseUrl { get; private set; }

        public string PersonalToken { get; private set; }

        public string BasicPassword { get; private set; }

        public string BasicUsername { get; private set; }

        public string Hash { get; private set; }

        public IEnumerable<Snippet> Snippets { get; private set; }

        public IEnumerable<Function> Functions { get; private set; }

        public IEnumerable<Rule> Rules { get; private set; }

        public IEnumerable<Policy> Policies { get; private set; }

        public bool Debug { get; set; }

        public RateLimit RateLimit { get; set; }

        public bool WhatIf { get; set; }

        public bool IgnoreSslErrors { get; set; }
    }
}

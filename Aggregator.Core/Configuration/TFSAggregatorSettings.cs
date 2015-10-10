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

    /// <summary>
    /// This class' represents Core settings as properties
    /// </summary>
    public partial class TFSAggregatorSettings
    {
        private static readonly char[] ListSeparators = new char[] { ',', ';' };

        public static TFSAggregatorSettings LoadFromFile(string settingsPath, ILogEvents logger)
        {
            DateTime lastWriteTime
                = System.IO.File.GetLastWriteTimeUtc(settingsPath);
            return Load(lastWriteTime, (xmlLoadOptions) => XDocument.Load(settingsPath, xmlLoadOptions), logger);
        }

        public static TFSAggregatorSettings LoadXml(string content, ILogEvents logger)
        {
            // conventional point in time reference
            DateTime staticTime = new DateTime(0, DateTimeKind.Utc);
            return LoadXml(content, staticTime, logger);
        }

        public static TFSAggregatorSettings LoadXml(string content, DateTime timestamp, ILogEvents logger)
        {
            return Load(timestamp, (xmlLoadOptions) => XDocument.Parse(content, xmlLoadOptions), logger);
        }

        /// <summary>
        /// Parse the specified <see cref="XDocument"/> to build a <see cref="TFSAggregatorSettings"/> instance.
        /// </summary>
        /// <param name="lastWriteTime">Last time the document has been changed.</param>
        /// <param name="load">A lambda returning the <see cref="XDocument"/> to parse.</param>
        /// <returns></returns>
        public static TFSAggregatorSettings Load(DateTime lastWriteTime, Func<LoadOptions, XDocument> load, ILogEvents logger)
        {
            var parser = new AggregatorSettingsXmlParser(logger);
            return parser.Parse(lastWriteTime, load);
        }

        public LogLevel LogLevel { get; private set; }

        public string ScriptLanguage { get; private set; }

        public bool AutoImpersonate { get; private set; }

        public string Hash { get; private set; }

        public IEnumerable<Rule> Rules { get; private set; }

        public IEnumerable<Policy> Policies { get; private set; }

        public bool Debug { get; set; }

        public RateLimit RateLimit { get; set; }
    }
}

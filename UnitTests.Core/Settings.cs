using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Aggregator.Core.Configuration;
using NSubstitute;
using Aggregator.Core;
using System.Xml.Schema;

namespace UnitTests.Core
{
    [TestClass]
    public class SettingsTests
    {
        [TestMethod]
        [ExpectedException(typeof(System.IO.FileNotFoundException))]
        public void Raise_error_for_non_existing_file()
        {
            var logger = Substitute.For<ILogEvents>();
            string file = "does_not_exists";

            var settings = TFSAggregatorSettings.LoadFromFile(file, logger);
        }

        [TestMethod]
        public void Should_load_full_syntax_with_no_errors()
        {
            var logger = Substitute.For<ILogEvents>();
            string file = "syntax.xml";

            var settings = TestHelpers.LoadConfigFromResourceFile(file, logger);

            Assert.IsNotNull(settings);
        }

        [TestMethod]
        public void Raise_error_for_configuration_invalid_loglevel()
        {
            var logger = Substitute.For<ILogEvents>();
            string config = @"<AggregatorConfiguration logLevel='Diag'></AggregatorConfiguration>";

            var settings = TFSAggregatorSettings.LoadXml(config, logger);

            logger.Received().InvalidConfiguration(
                XmlSeverityType.Error,
                "The 'logLevel' attribute is invalid - The value 'Diag' is invalid according to its datatype 'String' - The Enumeration constraint failed.",
                1, 26);
        }
    }
}

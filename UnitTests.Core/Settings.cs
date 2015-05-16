namespace UnitTests.Core
{
    using System.IO;
    using System.Xml.Schema;

    using Aggregator.Core;
    using Aggregator.Core.Configuration;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using NSubstitute;

    [TestClass]
    public class SettingsTests
    {
        [TestMethod]
        [ExpectedException(typeof(FileNotFoundException))]
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
        public void Log_error_for_configuration_invalid_loglevel()
        {
            var logger = Substitute.For<ILogEvents>();
            string config = @"<AggregatorConfiguration><runtime><logging level='Diag'/></runtime></AggregatorConfiguration>";

            var settings = TFSAggregatorSettings.LoadXml(config, logger);

            Assert.IsNull(settings);
            logger.Received().InvalidConfiguration(
                XmlSeverityType.Error,
                "The 'level' attribute is invalid - The value 'Diag' is invalid according to its datatype 'String' - The Enumeration constraint failed.",
                1, 44);
        }

        [TestMethod]
        public void Log_error_for_mismatched_rule_name()
        {
            var logger = Substitute.For<ILogEvents>();
            string config = @"
<AggregatorConfiguration>
    <rule name='r1'/>
    <policy name='p1'>
        <ruleRef name='r2'/>
    </policy>   
</AggregatorConfiguration>";

            var settings = TFSAggregatorSettings.LoadXml(config, logger);

            Assert.IsNull(settings);
            logger.Received().InvalidConfiguration(
                XmlSeverityType.Error,
                "Reference to undeclared ID is 'r2'.",
                5, 18);
        }

        [TestMethod]
        public void Log_error_for_policy_with_no_rules()
        {
            var logger = Substitute.For<ILogEvents>();
            string config = @"
<AggregatorConfiguration>
    <rule name='r1'/>
    <policy name='p1'/>
</AggregatorConfiguration>";

            var settings = TFSAggregatorSettings.LoadXml(config, logger);

            Assert.IsNull(settings);
            logger.Received().InvalidConfiguration(
                XmlSeverityType.Error,
                "The element 'policy' has incomplete content. List of possible elements expected: 'collectionScope, templateScope, projectScope, ruleRef'.",
                4, 6);
        }

        [TestMethod]
        public void Log_warning_for_unused_rule()
        {
            var logger = Substitute.For<ILogEvents>();
            string config = @"
<AggregatorConfiguration>
    <rule name='r1'/>
    <rule name='r2'/>
    <policy name='p1'>
        <ruleRef name='r2'/>
    </policy>   
</AggregatorConfiguration>";

            var settings = TFSAggregatorSettings.LoadXml(config, logger);

            Assert.IsNotNull(settings);
            logger.Received().UnreferencedRule("r1");
        }
    }
}

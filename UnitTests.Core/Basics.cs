namespace UnitTests.Core
{
    using Aggregator.Core;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using NSubstitute;

    [TestClass]
    public class Basics
    {
        [TestMethod]
        public void Can_load_a_fake_xml_configuration()
        {
            var logger = Substitute.For<ILogEvents>();

            var settings = TestHelpers.LoadConfigFromResourceFile("NoOp.policies", logger);
            var level = settings.LogLevel;

            Assert.AreEqual(LogLevel.Diagnostic, level);
        }
    }
}

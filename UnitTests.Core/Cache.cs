namespace UnitTests.Core
{
    using Aggregator.Core;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using NSubstitute;
    using System;
    using System.IO;

    [TestClass]
    public class Cache
    {
        DateTime referenceDate = new DateTime(2015, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        string SetupSettingsFile(string name)
        {
            string sourcePath = Path.Combine(@"..\..\ConfigurationsForTests", name);
            string destPath = Path.GetTempFileName();
            File.Copy(sourcePath, destPath, true);
            File.SetLastWriteTimeUtc(destPath, referenceDate);
            return destPath;
        }

        [TestMethod]
        public void Cache_cold_succeeded()
        {
            var logger = Substitute.For<ILogEvents>();
            var cache = new ObjectCache(logger);
            string path = SetupSettingsFile("NoOp.policies");

            var settings = cache.GetSettings(path);
            var level = settings.LogLevel;

            Assert.AreEqual(LogLevel.Diagnostic, level);
            logger.Received().ConfigurationChanged(path, DateTime.MinValue, referenceDate);
        }

        [TestMethod]
        public void Cache_warm_succeeded()
        {
            var logger = Substitute.For<ILogEvents>();
            var cache = new ObjectCache(logger);
            string path = SetupSettingsFile("NoOp.policies");

            var settings = cache.GetSettings(path);
            settings = cache.GetSettings(path);

            logger.Received().ConfigurationChanged(path, DateTime.MinValue, referenceDate);
            logger.Received().UsingCachedConfiguration(path, referenceDate, referenceDate);
        }

        [TestMethod]
        public void Cache_changed_succeeded()
        {
            var logger = Substitute.For<ILogEvents>();
            var cache = new ObjectCache(logger);
            string path = SetupSettingsFile("NoOp.policies");

            var settings = cache.GetSettings(path);
            var now = DateTime.UtcNow;
            File.SetLastWriteTimeUtc(path, now);
            settings = cache.GetSettings(path);

            logger.Received().ConfigurationChanged(path, DateTime.MinValue, referenceDate);
            logger.Received().ConfigurationChanged(path, referenceDate, now);
        }
    }
}

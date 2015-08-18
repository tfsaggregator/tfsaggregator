using Aggregator.Core.Context;
using Aggregator.Core.Interfaces;
using Aggregator.Core.Monitoring;

namespace UnitTests.Core
{
    using Aggregator.Core;
    using Aggregator.Core.Configuration;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using NSubstitute;
    using System;
    using System.IO;
    using System.Runtime.Caching;

    [TestClass]
    public class ContextCache
    {
        DateTime referenceDate = new DateTime(2015, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        string SetupSettingsFile(string sourceName)
        {
            string sourcePath = Path.Combine(@"..\..\ConfigurationsForTests", sourceName);
            string destPath = Path.GetTempFileName();
            File.Copy(sourcePath, destPath, true);
            File.SetLastWriteTimeUtc(destPath, referenceDate);
            return destPath;
        }

        [TestMethod]
        public void ContextCache_cold_succeeds()
        {
            string path = SetupSettingsFile("NoOp.policies");

            var logger = Substitute.For<ILogEvents>();
            var context = Substitute.For<IRequestContext>();
            var runtime = RuntimeContext.GetContext(()=> path, context, logger);

            var level = runtime.Settings.LogLevel;

            Assert.AreEqual(LogLevel.Diagnostic, level);
            //logger.Received().LoadingConfiguration(path);
        }

        [TestMethod]
        public void ContextCache_warm_succeeds()
        {
            string path = SetupSettingsFile("NoOp.policies");

            var logger = Substitute.For<ILogEvents>();
            var context = Substitute.For<IRequestContext>();
            var runtime1 = RuntimeContext.GetContext(() => path, context, logger);
            var runtime2 = RuntimeContext.GetContext(() => path, context, logger);

            Assert.AreEqual(runtime1.Hash, runtime2.Hash);
        }

        [TestMethod]
        [Ignore]
        public void ContextCache_file_changed_succeeds()
        {
            string sourcePath = @"..\..\ConfigurationsForTests\NoOp.policies";
            string destPath = Path.GetTempFileName();
            File.Copy(sourcePath, destPath, true);
            File.SetLastWriteTimeUtc(destPath, referenceDate);

            var logger = Substitute.For<ILogEvents>();
            var context = Substitute.For<IRequestContext>();
            var runtime1 = RuntimeContext.GetContext(() => destPath, context, logger);

            File.SetLastWriteTimeUtc(destPath, DateTime.UtcNow);
            // this delay is ok while Debugging
            // in Run, it *never* works
            Pause();

            var runtime2 = RuntimeContext.GetContext(() => destPath, context, logger);

            Assert.AreNotEqual(runtime1.Hash, runtime2.Hash);
        }

        // gives HostFileChangeMonitor a chance to sense the change
        private static void Pause()
        {
            for (int i = 0; i < 2; i++)
            {
                System.Windows.Forms.Application.DoEvents();
                System.Threading.Tasks.Task.Factory.StartNew(
                    () => System.Threading.Thread.Sleep(300)
                    ).Wait();
                System.Threading.Thread.Sleep(300);
            }
        }
    }
}

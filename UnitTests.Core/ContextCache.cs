using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

using Aggregator.Core.Context;
using Aggregator.Core.Interfaces;
using Aggregator.Core.Monitoring;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using NSubstitute;

namespace UnitTests.Core
{
    [TestClass]
    public class ContextCache
    {
        private readonly DateTime referenceDate = new DateTime(2015, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        private string SetupSettingsFile(string sourceName)
        {
            string sourcePath = Path.Combine(@"..\..\..\ConfigurationsForTests", sourceName);
            string destPath = Path.GetTempFileName();
            File.Copy(sourcePath, destPath, true);
            File.SetLastWriteTimeUtc(destPath, this.referenceDate);
            return destPath;
        }

        [TestMethod]
        public void ContextCache_cold_succeeds()
        {
            string path = this.SetupSettingsFile("NoOp.policies");

            var logger = Substitute.For<ILogEvents>();
            var context = Substitute.For<IRequestContext>();
            var runtime = RuntimeContext.GetContext(() => path, context, logger);

            var level = runtime.Settings.LogLevel;

            Assert.AreEqual(LogLevel.Diagnostic, level);
        }

        [TestMethod]
        public void ContextCache_warm_succeeds()
        {
            string path = this.SetupSettingsFile("NoOp.policies");

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
            File.SetLastWriteTimeUtc(destPath, this.referenceDate);

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
                Application.DoEvents();
                Task.Factory.StartNew(
                    () => Thread.Sleep(300))
                    .Wait();
                Thread.Sleep(300);
            }
        }
    }
}

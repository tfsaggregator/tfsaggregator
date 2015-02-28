using Aggregator.Core;
using Microsoft.TeamFoundation.Framework.Server;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitTests.Core
{
    [TestClass]
    public class Basics
    {
        [TestMethod]
        public void Can_load_a_fake_xml_configuration()
        {
            var settings = TestHelpers.LoadConfigFromResourceFile("NoOp.policies");
            var level = settings.LogLevel;
            Assert.AreEqual(LogLevel.Diagnostic, level);
        }

        [TestMethod]
        public void Can_execute_a_script_interacting_with_an_object_model()
        {
            //string script = @" $self.Fields[""z""].Value ";
            string script = @" $z = $self.Fields['z']; $z.Value ";
            var repository = Substitute.For<IWorkItemRepository>();
            var workItem = Substitute.For<IWorkItem>();
            workItem.Id.Returns(1);
            workItem.Fields["z"].Value.Returns(42);
            repository.GetWorkItem(1).Returns(workItem);
            var logger = Substitute.For<ILogEvents>();
            var engine = new ScriptEngine("test", script, repository, logger);
            //sanity check
            Assert.AreEqual(42, workItem.Fields["z"].Value);

            engine.Run(workItem);

            /*var expected = new System.Collections.ObjectModel.Collection<System.Management.Automation.PSObject>();
            expected.Add(new System.Management.Automation.PSObject(42));
            logger.Received().ResultsFromScriptRun("test", expected);
             */
        }

        [TestMethod]
        public void Can_execute_a_noop_rule()
        {
            var settings = TestHelpers.LoadConfigFromResourceFile("NoOp.policies");
            var repository = Substitute.For<IWorkItemRepository>();
            var workItem = Substitute.For<IWorkItem>();
            workItem.TypeName.Returns("Task");
            workItem.IsValid().Returns(true);
            repository.GetWorkItem(1).Returns(workItem);
            var logger = Substitute.For<ILogEvents>();
            var processor = new EventProcessor(repository, logger);
            var context = Substitute.For<IRequestContext>();
            var notification = Substitute.For<INotification>();
            notification.WorkItemId.Returns(1);

            var result = processor.ProcessEvent(context, notification, settings);

            Assert.AreEqual(0, result.ExceptionProperties.Count());
            workItem.Received().Save();
            Assert.AreEqual(EventNotificationStatus.ActionPermitted, result.NotificationStatus);
        }

        [TestMethod]
        [Ignore]
        public void Can_execute_a_noop_rule_100_times()
        {
            var settings = TestHelpers.LoadConfigFromResourceFile("NoOp.policies");
            var repository = Substitute.For<IWorkItemRepository>();
            var workItem = Substitute.For<IWorkItem>();
            workItem.TypeName.Returns("Task");
            workItem.IsValid().Returns(true);
            repository.GetWorkItem(1).Returns(workItem);
            var logger = Substitute.For<ILogEvents>();
            var processor = new EventProcessor(repository, logger);
            var context = Substitute.For<IRequestContext>();
            var notification = Substitute.For<INotification>();
            notification.WorkItemId.Returns(1);

            ProcessingResult result = null;
            for (int i = 0; i < 100; i++)
            {
                result = processor.ProcessEvent(context, notification, settings);
            }

            Assert.AreEqual(0, result.ExceptionProperties.Count());

            var expected = new System.Collections.ObjectModel.Collection<System.Management.Automation.PSObject>();
            expected.Add(new System.Management.Automation.PSObject(42));
            logger.Received().ResultsFromScriptRun("Noop", expected);
            workItem.Received().Save();
            Assert.AreEqual(EventNotificationStatus.ActionPermitted, result.NotificationStatus);
        }
    }
}

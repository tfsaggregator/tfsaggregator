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
        [TestCategory("CSharpScript")]
        public void Can_run_a_CSharp_script_interacting_with_a_verbose_object_model()
        {
            string script = @"
self.Fields[""x""].Value = 33;
return self.Fields[""z""].Value;
";
            var repository = Substitute.For<IWorkItemRepository>();
            var workItem = Substitute.For<IWorkItem>();
            var xField = Substitute.For<IFieldWrapper>();
            var zField = Substitute.For<IFieldWrapper>();
            workItem.Id.Returns(1);
            xField.OriginalValue.Returns(11);
            workItem.Fields["x"] = xField;
            workItem.Fields["z"] = zField;
            zField.Value.Returns(42);
            repository.GetWorkItem(1).Returns(workItem);
            var logger = Substitute.For<ILogEvents>();
            var engine = new CSharpScriptEngine("test", script, repository, logger);

            engine.Run(workItem);

            //logger.DidNotReceiveWithAnyArgs().ScriptHasError();
            Assert.AreEqual(33, xField.Value);
            object expected = 42;
            logger.Received().ResultsFromScriptRun("test", expected);
        }

        [TestMethod]
        [TestCategory("CSharpScript")]
        public void Can_run_a_CSharp_script_interacting_with_a_shorthand_object_model()
        {
            string script = @"
self[""x""] = 33;
return self[""z""];
";
            var repository = Substitute.For<IWorkItemRepository>();
            var workItem = Substitute.For<IWorkItem>();
            workItem.Id.Returns(1);
            workItem["x"] = 11;
            workItem["z"].Returns(42);
            repository.GetWorkItem(1).Returns(workItem);
            var logger = Substitute.For<ILogEvents>();
            var engine = new CSharpScriptEngine("test", script, repository, logger);

            engine.Run(workItem);

            //logger.DidNotReceiveWithAnyArgs().ScriptHasError();
            Assert.AreEqual(33, workItem["x"]);
            object expected = 42;
            logger.Received().ResultsFromScriptRun("test", expected);
        }

        [TestMethod]
        [TestCategory("VBNetScript")]
        public void Can_run_a_VBNet_script_interacting_with_an_object_model()
        {
            string script = @"
self(""x"") = 33
return self(""z"")
";
            var repository = Substitute.For<IWorkItemRepository>();
            var workItem = Substitute.For<IWorkItem>();
            workItem.Id.Returns(1);
            workItem["x"] = 11;
            workItem["z"].Returns(42);
            repository.GetWorkItem(1).Returns(workItem);
            var logger = Substitute.For<ILogEvents>();
            logger.WhenForAnyArgs(c => System.Diagnostics.Debug.WriteLine(c));
            var engine = new VBNetScriptEngine("test", script, repository, logger);

            engine.Run(workItem);

            //logger.DidNotReceiveWithAnyArgs().ScriptHasError();
            Assert.AreEqual(33, workItem["x"]);
            object expected = 42;
            logger.Received().ResultsFromScriptRun("test", expected);
        }

        [TestMethod]
        [TestCategory("Powershell")]
        [Ignore] // Will fail as PS do not understand Castle's proxies
        public void Can_run_a_Powershell_script_interacting_with_an_object_model()
        {
            string script = @" $self.Fields[""z""].Value ";
            var repository = Substitute.For<IWorkItemRepository>();
            var workItem = Substitute.For<IWorkItem>();
            workItem.Id.Returns(1);
            workItem.Fields["z"].Value.Returns(42);
            repository.GetWorkItem(1).Returns(workItem);
            var logger = Substitute.For<ILogEvents>();
            var engine = new PsScriptEngine("test", script, repository, logger);
            //sanity check
            Assert.AreEqual(42, workItem.Fields["z"].Value);

            engine.Run(workItem);

            var expected = new System.Collections.ObjectModel.Collection<System.Management.Automation.PSObject>();
            expected.Add(new System.Management.Automation.PSObject(42));
            logger.Received().ResultsFromScriptRun("test", expected);
        }

        [TestMethod]
        [TestCategory("Powershell")]
        public void Can_execute_a_Powershell_noop_rule()
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
    }
}

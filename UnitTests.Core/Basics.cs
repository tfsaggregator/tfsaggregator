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
    using System.Collections.ObjectModel;
    using System.Management.Automation;



    using UnitTests.Core.Mock;

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
        public void CS__Can_run_a_script_interacting_with_an_object_model()
        {
            string script = @"
self.Fields[""x""].Value = 33;
return self.Fields[""z""].Value;
";
            var repository = Substitute.For<IWorkItemRepository>();
            var workItem = Substitute.For<IWorkItem>();
            workItem.Id.Returns(1);
            workItem.Fields["x"].Value = 0;
            workItem.Fields["z"].Value.Returns(42);
            repository.GetWorkItem(1).Returns(workItem);
            var logger = Substitute.For<ILogEvents>();
            var engine = new CsScriptEngine("test", script, repository, logger);
            //sanity check
            Assert.AreEqual(42, workItem.Fields["z"].Value);

            engine.Run(workItem);

            var expected = 42;
            Assert.AreEqual(33, workItem.Fields["x"].Value);
            logger.Received().ResultsFromScriptRun("test", expected);
        }

        [TestMethod]
        [TestCategory("Powershell")]
        public void PS__Can_run_a_script_interacting_with_an_object_model()
        {
            string script = @" 

$self.Fields[""x""].Value = 33
return $self.Fields[""z""].Value ";

            var repository = new WorkItemRepositoryMock();
            var workItem = new WorkItemMock();

            workItem.Fields["z"].Value = 42;
            workItem.Fields["x"].Value = 0;
            workItem.Id = 1;
            
            repository.SetWorkItems(new []{workItem});
            var logger = Substitute.For<ILogEvents>();

            Assert.IsNotNull((repository.GetWorkItem(1)));
            
            var engine = new PsScriptEngine("test", script, repository, logger);
            //sanity check
            Assert.AreEqual(42, workItem.Fields["z"].Value);

            engine.Run(workItem);

            var expected = new Collection<PSObject>();
            expected.Add(new PSObject(42));

            Assert.AreEqual(33, workItem.Fields["x"].Value);

            logger.Received().ResultsFromScriptRun(
                "test", 
                Arg.Is<Collection<PSObject>>(x => x.Select(o => o.BaseObject).SequenceEqual(expected.Select(o => o.BaseObject)))
            );

            
        }

        [TestMethod]
        [TestCategory("Powershell")]
        public void PS__Can_run_a_script_returning_a_value()
        {
            string script = @" return $Id ";

            var repository = new WorkItemRepositoryMock();
            var workItem = new WorkItemMock();

            workItem.Id = 1;

            repository.SetWorkItems(new[] { workItem });
            var logger = Substitute.For<ILogEvents>();

            Assert.IsNotNull((repository.GetWorkItem(1)));

            var engine = new PsScriptEngine("test", script, repository, logger);
            //sanity check
            

            engine.Run(workItem);

            var expected = new Collection<PSObject>();
            expected.Add(new PSObject(1));

            logger.Received().ResultsFromScriptRun(
                "test",
                Arg.Is<Collection<PSObject>>(x => x.Select(o => o.BaseObject).SequenceEqual(expected.Select(o => o.BaseObject)))
            );
        }

        [TestMethod]
        [TestCategory("Powershell")]
        public void PS__Can_execute_a_noop_rule()
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

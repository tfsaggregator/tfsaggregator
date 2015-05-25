namespace UnitTests.Core
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Linq;
    using System.Management.Automation;

    using Aggregator.Core;

    using Microsoft.TeamFoundation.Framework.Server;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using NSubstitute;

    using UnitTests.Core.Mock;

    [TestClass]
    public class ScriptEngines
    {
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
            var engine = new CSharpScriptEngine(repository, logger);
            engine.LoadAndRun("test", script, workItem);

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
            var engine = new CSharpScriptEngine(repository, logger);
            engine.LoadAndRun("test", script, workItem);

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
            logger.WhenForAnyArgs(c => Debug.WriteLine(c));
            var engine = new VBNetScriptEngine(repository, logger);

            engine.LoadAndRun("test", script, workItem);

            Assert.AreEqual(33, workItem["x"]);
            object expected = 42;
            logger.Received().ResultsFromScriptRun("test", expected);
        }

        [TestMethod]
        [TestCategory("Powershell")]
        public void Can_run_a_Powershell_script_interacting_with_an_object_model()
        {
            string script = @" 

$self.Fields[""x""].Value = 33
return $self.Fields[""z""].Value ";

            var repository = new WorkItemRepositoryMock();
            var workItem = new WorkItemMock(repository);

            workItem.Fields["z"].Value = 42;
            workItem.Fields["x"].Value = 0;
            workItem.Id = 1;
            
            repository.SetWorkItems(new []{workItem});
            var logger = Substitute.For<ILogEvents>();

            Assert.IsNotNull((repository.GetWorkItem(1)));
            
            var engine = new PsScriptEngine(repository, logger);
            //sanity check
            Assert.AreEqual(42, workItem.Fields["z"].Value);

            engine.LoadAndRun("test", script, workItem);

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
        public void Can_run_a_Powershell_script_returning_a_value()
        {
            string script = @" return $self.Id ";

            var repository = new WorkItemRepositoryMock();
            var workItem = new WorkItemMock(repository);

            workItem.Id = 1;

            repository.SetWorkItems(new[] { workItem });
            var logger = Substitute.For<ILogEvents>();

            Assert.IsNotNull((repository.GetWorkItem(1)));

            var engine = new PsScriptEngine(repository, logger);

            engine.LoadAndRun("test", script, workItem);

            var expected = new Collection<PSObject>();
            expected.Add(new PSObject(1));

            logger.Received().ResultsFromScriptRun(
                "test",
                Arg.Is<Collection<PSObject>>(x => x.Select(o => o.BaseObject).SequenceEqual(expected.Select(o => o.BaseObject)))
            );
        }

        [TestMethod]
        [TestCategory("Powershell")]
        public void Can_execute_a_Powershell_noop_rule()
        {
            var logger = Substitute.For<ILogEvents>();
            var settings = TestHelpers.LoadConfigFromResourceFile("NoOp.policies", logger);
            var repository = Substitute.For<IWorkItemRepository>();
            var workItem = Substitute.For<IWorkItem>();
            var cache = new ObjectCacheMock(logger, settings);
            var processor = new EventProcessor(repository, cache, settings);
            var context = Substitute.For<IRequestContext>();
            var notification = Substitute.For<INotification>();
            notification.WorkItemId.Returns(1);
            repository.LoadedWorkItems.Returns(new ReadOnlyCollection<IWorkItem>(new List<IWorkItem>() { workItem }));

            var result = processor.ProcessEvent(context, notification);

            Assert.AreEqual(0, result.ExceptionProperties.Count());
            Assert.AreEqual(EventNotificationStatus.ActionPermitted, result.NotificationStatus);
        }
    }
}

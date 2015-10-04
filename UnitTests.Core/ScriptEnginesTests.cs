using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Management.Automation;

using Aggregator.Core;
using Aggregator.Core.Context;
using Aggregator.Core.Interfaces;
using Aggregator.Core.Monitoring;

using Microsoft.TeamFoundation.Framework.Server;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using NSubstitute;

using UnitTests.Core.Mock;

using Debugger = System.Diagnostics.Debugger;

namespace UnitTests.Core
{
    [TestClass]
    public class ScriptEnginesTests
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
            var engine = new CSharpScriptEngine(logger, Debugger.IsAttached);
            engine.LoadAndRun("test", script, workItem, repository);

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
            var engine = new CSharpScriptEngine(logger, Debugger.IsAttached);
            engine.LoadAndRun("test", script, workItem, repository);

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
            var engine = new VBNetScriptEngine(logger, Debugger.IsAttached);

            engine.LoadAndRun("test", script, workItem, repository);

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

            repository.SetWorkItems(new[] { workItem });
            var logger = Substitute.For<ILogEvents>();

            Assert.IsNotNull(repository.GetWorkItem(1));

            var engine = new PsScriptEngine(logger, Debugger.IsAttached);

            // sanity check
            Assert.AreEqual(42, workItem.Fields["z"].Value);

            engine.LoadAndRun("test", script, workItem, repository);

            var expected = new Collection<PSObject> { new PSObject(42) };

            Assert.AreEqual(33, workItem.Fields["x"].Value);

            logger.Received().ResultsFromScriptRun(
                "test",
                Arg.Is<Collection<PSObject>>(x => x.Select(o => o.BaseObject).SequenceEqual(expected.Select(o => o.BaseObject))));
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

            Assert.IsNotNull(repository.GetWorkItem(1));

            var engine = new PsScriptEngine(logger, Debugger.IsAttached);

            engine.LoadAndRun("test", script, workItem, repository);

            var expected = new Collection<PSObject> { new PSObject(1) };

            logger.Received().ResultsFromScriptRun(
                "test",
                Arg.Is<Collection<PSObject>>(x => x.Select(o => o.BaseObject).SequenceEqual(expected.Select(o => o.BaseObject))));
        }

        [TestMethod]
        [TestCategory("Powershell")]
        public void Can_execute_a_Powershell_noop_rule()
        {
            var logger = Substitute.For<ILogEvents>();
            var settings = TestHelpers.LoadConfigFromResourceFile("NoOp.policies", logger);
            var repository = Substitute.For<IWorkItemRepository>();
            var workItem = Substitute.For<IWorkItem>();
            var context = Substitute.For<IRequestContext>();
            context.GetProjectCollectionUri().Returns(
                new System.Uri("http://localhost:8080/tfs/DefaultCollection"));
            var runtime = RuntimeContext.MakeRuntimeContext("settingsPath", settings, context, logger, (c, i, l) => repository);
            using (var processor = new EventProcessor(runtime))
            {
                var notification = Substitute.For<INotification>();
                notification.WorkItemId.Returns(1);
                repository.LoadedWorkItems.Returns(
                    new ReadOnlyCollection<IWorkItem>(new List<IWorkItem>() { workItem }));
                repository.CreatedWorkItems.Returns(
                    new ReadOnlyCollection<IWorkItem>(new List<IWorkItem>()));

                var result = processor.ProcessEvent(context, notification);

                Assert.AreEqual(0, result.ExceptionProperties.Count());
                Assert.AreEqual(EventNotificationStatus.ActionPermitted, result.NotificationStatus);
            }
        }

        [TestMethod]
        [TestCategory("CSharpScript")]
        public void Can_run_a_CSharp_script_with_logging()
        {
            string script = @"
logger.Log(""Test"");
";
            var repository = Substitute.For<IWorkItemRepository>();
            var workItem = Substitute.For<IWorkItem>();
            var logger = Substitute.For<ILogEvents>();
            logger.ScriptLogger = Substitute.For<IRuleLogger>();
            var engine = new CSharpScriptEngine(logger, Debugger.IsAttached);
            engine.LoadAndRun("test", script, workItem, repository);
            logger.ScriptLogger.Received().Log("Test");
        }

        [TestMethod]
        [TestCategory("VBNetScript")]
        public void Can_run_a_VBNet_script_with_logging()
        {
            string script = @"
logger.Log(""Test"")
";
            var repository = Substitute.For<IWorkItemRepository>();
            var workItem = Substitute.For<IWorkItem>();
            var logger = Substitute.For<ILogEvents>();
            logger.ScriptLogger = Substitute.For<IRuleLogger>();
            var engine = new VBNetScriptEngine(logger, Debugger.IsAttached);
            engine.LoadAndRun("test", script, workItem, repository);
            logger.ScriptLogger.Received().Log("Test");
        }

        [TestMethod]
        [TestCategory("CSharpScript")]
        public void Can_CSharp_use_Linq()
        {
            string script = @"
int[] array = { 1, 3, 5, 7 };
return (int)array.Average();
";
            var repository = Substitute.For<IWorkItemRepository>();
            var workItem = Substitute.For<IWorkItem>();
            repository.GetWorkItem(1).Returns(workItem);
            var logger = Substitute.For<ILogEvents>();
            var engine = new CSharpScriptEngine(logger, Debugger.IsAttached);
            engine.LoadAndRun("test", script, workItem, repository);
            object expected = 4;
            logger.Received().ResultsFromScriptRun("test", expected);
        }

        [TestMethod]
        [TestCategory("VBNetScript")]
        public void Can_VBNet_use_Linq()
        {
            string script = @"
Dim array As Integer() = {1, 3, 5, 7}
Return CInt(array.Average())
";
            var repository = Substitute.For<IWorkItemRepository>();
            var workItem = Substitute.For<IWorkItem>();
            repository.GetWorkItem(1).Returns(workItem);
            var logger = Substitute.For<ILogEvents>();
            var engine = new VBNetScriptEngine(logger, Debugger.IsAttached);
            engine.LoadAndRun("test", script, workItem, repository);
            object expected = 4;
            logger.Received().ResultsFromScriptRun("test", expected);
        }

        [TestMethod]
        [TestCategory("CSharpScript")]
        public void Catch_CSharp_rule_compile_error()
        {
            string good_script = @"
logger.Log(""Test"");
";
            string bad_script = @"
loger.Log(""Test"");
";
            var logger = Substitute.For<ILogEvents>();
            var engine = new CSharpScriptEngine(logger, Debugger.IsAttached);

            engine.Load("good", good_script);
            engine.Load("bad", bad_script);
            engine.LoadCompleted();

            logger.Received().ScriptHasError("bad", 2, 1, "CS0103", "The name 'loger' does not exist in the current context");
        }

        [TestMethod]
        [TestCategory("VBNetScript")]
        public void Catch_VBNet_rule_compile_error()
        {
            string good_script = @"
logger.Log(""Test"")
";
            string bad_script = @"
loger.Log(""Test"")
";
            var logger = Substitute.For<ILogEvents>();
            var engine = new VBNetScriptEngine(logger, Debugger.IsAttached);

            engine.Load("good", good_script);
            engine.Load("bad", bad_script);
            engine.LoadCompleted();

            logger.Received().ScriptHasError("bad", 2, 0, "BC30451", "'loger' is not declared. It may be inaccessible due to its protection level.");
        }
    }
}

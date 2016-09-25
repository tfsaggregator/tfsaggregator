using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Management.Automation;

using Aggregator.Core;
using Aggregator.Core.Context;
using Aggregator.Core.Interfaces;
using Aggregator.Core.Monitoring;
using Aggregator.Core.Script;

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
            var xField = Substitute.For<IField>();
            var zField = Substitute.For<IField>();
            workItem.Id.Returns(1);
            xField.OriginalValue.Returns(11);
            workItem.Fields["x"] = xField;
            workItem.Fields["z"] = zField;
            zField.Value.Returns(42);
            repository.GetWorkItem(1).Returns(workItem);
            var logger = Substitute.For<ILogEvents>();
            var library = Substitute.For<IScriptLibrary>();
            var engine = new CSharpScriptEngine(logger, Debugger.IsAttached, library);
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
            var library = Substitute.For<IScriptLibrary>();
            var engine = new CSharpScriptEngine(logger, Debugger.IsAttached, library);
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
            var library = Substitute.For<IScriptLibrary>();
            var engine = new VBNetScriptEngine(logger, Debugger.IsAttached, library);

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

            var logger = Substitute.For<ILogEvents>();
            var settings = TestHelpers.LoadConfigFromResourceFile("NewObjects.policies", logger);
            var repository = new WorkItemRepositoryMock();
            System.Func<IRuntimeContext, IScriptLibrary> scriptLibraryBuilder = (x) => Substitute.For<IScriptLibrary>();
            var context = Substitute.For<IRequestContext>();
            context.GetProjectCollectionUri().Returns(
                new System.Uri("http://localhost:8080/tfs/DefaultCollection"));
            var runtime = RuntimeContext.MakeRuntimeContext("settingsPath", settings, context, logger, (c) => repository, scriptLibraryBuilder);

            var workItem = new WorkItemMock(repository, runtime);

            workItem.Fields["z"].Value = 42;
            workItem.Fields["x"].Value = 0;
            workItem.Id = 1;

            repository.SetWorkItems(new[] { workItem });

            Assert.IsNotNull(repository.GetWorkItem(1));
            var library = Substitute.For<IScriptLibrary>();

            var engine = new PsScriptEngine(logger, Debugger.IsAttached, library);

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

            var logger = Substitute.For<ILogEvents>();
            var settings = TestHelpers.LoadConfigFromResourceFile("NewObjects.policies", logger);
            var repository = new WorkItemRepositoryMock();
            System.Func<IRuntimeContext, IScriptLibrary> scriptLibraryBuilder = (x) => Substitute.For<IScriptLibrary>();
            var context = Substitute.For<IRequestContext>();
            context.GetProjectCollectionUri().Returns(
                new System.Uri("http://localhost:8080/tfs/DefaultCollection"));
            var runtime = RuntimeContext.MakeRuntimeContext("settingsPath", settings, context, logger, (c) => repository, scriptLibraryBuilder);

            var workItem = new WorkItemMock(repository, runtime);

            workItem.Id = 1;

            repository.SetWorkItems(new[] { workItem });

            Assert.IsNotNull(repository.GetWorkItem(1));
            var library = Substitute.For<IScriptLibrary>();

            var engine = new PsScriptEngine(logger, Debugger.IsAttached, library);

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
            System.Func<IRuntimeContext, IScriptLibrary> scriptLibraryBuilder = (x) => Substitute.For<IScriptLibrary>();
            var workItem = Substitute.For<IWorkItem>();
            var context = Substitute.For<IRequestContext>();
            context.GetProjectCollectionUri().Returns(
                new System.Uri("http://localhost:8080/tfs/DefaultCollection"));
            var runtime = RuntimeContext.MakeRuntimeContext(@"C:\Can_execute_a_Powershell_noop_rule", settings, context, logger, (c) => repository, scriptLibraryBuilder);
            using (var processor = new EventProcessor(runtime))
            {
                var notification = Substitute.For<INotification>();
                notification.WorkItemId.Returns(1);
                repository.LoadedWorkItems.Returns(
                    new ReadOnlyCollection<IWorkItem>(new List<IWorkItem>() { workItem }));
                repository.CreatedWorkItems.Returns(
                    new ReadOnlyCollection<IWorkItem>(new List<IWorkItem>()));

                var result = processor.ProcessEvent(context, notification);

                Assert.AreEqual(0, result.ExceptionProperties.Count);
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
            var library = Substitute.For<IScriptLibrary>();
            var engine = new CSharpScriptEngine(logger, Debugger.IsAttached, library);
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
            var library = Substitute.For<IScriptLibrary>();
            var engine = new VBNetScriptEngine(logger, Debugger.IsAttached, library);
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
            var library = Substitute.For<IScriptLibrary>();
            var engine = new CSharpScriptEngine(logger, Debugger.IsAttached, library);
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
            var library = Substitute.For<IScriptLibrary>();
            var engine = new VBNetScriptEngine(logger, Debugger.IsAttached, library);
            engine.LoadAndRun("test", script, workItem, repository);
            object expected = 4;
            logger.Received().ResultsFromScriptRun("test", expected);
        }

        [TestMethod]
        [TestCategory("CSharpScript")]
        public void Catch_CSharp_rule_compile_error()
        {
            var good_script = new ScriptSourceElement()
            {
                Name = "good",
                Type = ScriptSourceElementType.Rule,
                SourceCode = @"
logger.Log(""Test"");
"
            };
            var bad_script = new ScriptSourceElement()
            {
                Name = "bad",
                Type = ScriptSourceElementType.Rule,
                SourceCode = @"
loger.Log(""Test"");
"
            };
            var logger = Substitute.For<ILogEvents>();
            var library = Substitute.For<IScriptLibrary>();
            var engine = new CSharpScriptEngine(logger, Debugger.IsAttached, library);

            engine.Load(new ScriptSourceElement[] { good_script, bad_script });

            logger.Received().ScriptHasError("bad", 2, 1, "CS0103", "The name 'loger' does not exist in the current context");
        }

        [TestMethod]
        [TestCategory("VBNetScript")]
        public void Catch_VBNet_rule_compile_error()
        {
            var good_script = new ScriptSourceElement()
            {
                Name = "good",
                Type = ScriptSourceElementType.Rule,
                SourceCode = @"
logger.Log(""Test"");
"
            };
            var bad_script = new ScriptSourceElement()
            {
                Name = "bad",
                Type = ScriptSourceElementType.Rule,
                SourceCode = @"
loger.Log(""Test"");
"
            };
            var logger = Substitute.For<ILogEvents>();
            var library = Substitute.For<IScriptLibrary>();
            var engine = new VBNetScriptEngine(logger, Debugger.IsAttached, library);

            engine.Load(new ScriptSourceElement[] { good_script, bad_script });

            logger.Received().ScriptHasError("bad", 2, 0, "BC30451", "'loger' is not declared. It may be inaccessible due to its protection level.");
        }

        [TestMethod]
        [TestCategory("CSharpScript")]
        public void Logger_overloads_work()
        {
            string script = @"
logger.Log(""Hello, World from {1} #{0}!"", self.Id, self.TypeName);
logger.Log(LogLevel.Warning, ""Unexpected work item state!"");
";
            var repository = Substitute.For<IWorkItemRepository>();
            var workItem = Substitute.For<IWorkItem>();
            workItem.Id.Returns(1);
            workItem.TypeName.Returns("Task");
            repository.GetWorkItem(1).Returns(workItem);
            var logger = Substitute.For<ILogEvents>();
            var library = Substitute.For<IScriptLibrary>();
            var engine = new CSharpScriptEngine(logger, Debugger.IsAttached, library);
            engine.LoadAndRun("test", script, workItem, repository);

            logger.Received().ScriptLogger.Log(LogLevel.Verbose, "test", "Hello, World from Task #1!");
            logger.Received().ScriptLogger.Log(LogLevel.Warning, "test", "Unexpected work item state!");
        }

        [TestMethod]
        [TestCategory("CSharpScript")]
        public void Can_CSharp_rule_use_snippet_and_function()
        {
            // Arrange
            var snippet1 = new ScriptSourceElement()
            {
                Name = "MySnippet",
                Type = ScriptSourceElementType.Snippet,
                SourceCode = @"
logger.Log(""This is MySnippet code."");
"
            };
            var function1 = new ScriptSourceElement()
            {
                Name = null,
                Type = ScriptSourceElementType.Function,
                SourceCode = @"
int MyFunc() { return 42; }
"
            };
            var rule1 = new ScriptSourceElement()
            {
                Name = "rule1",
                Type = ScriptSourceElementType.Rule,
                SourceCode = @"
${MySnippet}
logger.Log(""Hello, World from {1} #{0}!"", self.Id, self.TypeName);
logger.Log(""MyFunc returns {0}."", MyFunc());
"
            };

            var logger = Substitute.For<ILogEvents>();
            var library = Substitute.For<IScriptLibrary>();
            var engine = new CSharpScriptEngine(logger, Debugger.IsAttached, library);
            engine.Load(new ScriptSourceElement[] { rule1, function1, snippet1 });

            var repository = Substitute.For<IWorkItemRepository>();
            var workItem = Substitute.For<IWorkItem>();
            workItem.Id.Returns(1);
            workItem.TypeName.Returns("Task");
            repository.GetWorkItem(1).Returns(workItem);

            // Act
            engine.Run(rule1.Name, workItem, repository);

            // Assert
            logger.Received().ScriptLogger.Log(LogLevel.Verbose, rule1.Name, "This is MySnippet code.");
            logger.Received().ScriptLogger.Log(LogLevel.Verbose, rule1.Name, "Hello, World from Task #1!");
            logger.Received().ScriptLogger.Log(LogLevel.Verbose, rule1.Name, "MyFunc returns 42.");
        }

        [TestMethod]
        [TestCategory("CSharpScript")]
        public void Library_SendMail_succeeds()
        {
            string script = @"
string to = ""test@example.com"";
string subject = ""Test from Rule"";
string body = ""It worked!"";
Library.SendMail(to, subject, body);
";
            var repository = Substitute.For<IWorkItemRepository>();
            var workItem = Substitute.For<IWorkItem>();
            workItem.Id.Returns(1);
            workItem.TypeName.Returns("Task");
            repository.GetWorkItem(1).Returns(workItem);
            var logger = Substitute.For<ILogEvents>();
            var library = Substitute.For<IScriptLibrary>();
            var engine = new CSharpScriptEngine(logger, Debugger.IsAttached, library);
            engine.LoadAndRun("test", script, workItem, repository);

            string to = "test@example.com";
            string subject = "Test from Rule";
            string body = "It worked!";
            library.Received().SendMail(to, subject, body);
        }
    }
}

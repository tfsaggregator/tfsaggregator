using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Aggregator.Core.Configuration;
using Aggregator.Core.Context;
using Aggregator.Core.Extensions;
using Aggregator.Core.Interfaces;
using Aggregator.Core.Monitoring;

namespace UnitTests.Core
{
    using Aggregator.Core;
    using Aggregator.Core.Navigation;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using NSubstitute;
    using UnitTests.Core.Mock;

    [TestClass]
    public class NavigationTests
    {
        private static WorkItemRepositoryMock MakeRepository(out IWorkItem startPoint)
        {
            var logger = Substitute.For<ILogEvents>();
            var settings = TestHelpers.LoadConfigFromResourceFile("NewObjects.policies", logger);
            var repository = new WorkItemRepositoryMock();
            System.Func<IRuntimeContext, IScriptLibrary> scriptLibraryBuilder = (x) => Substitute.For<IScriptLibrary>();
            var context = Substitute.For<IRequestContext>();
            context.GetProjectCollectionUri().Returns(
                new System.Uri("http://localhost:8080/tfs/DefaultCollection"));
            var runtime = RuntimeContext.MakeRuntimeContext("settingsPath", settings, context, logger, (c) => repository, scriptLibraryBuilder);

            var grandParent = new WorkItemMock(repository, runtime);
            grandParent.Id = 1;
            grandParent.TypeName = "Requirement";
            grandParent["Microsoft.VSTS.Scheduling.RemainingWork"] = 2.0;

            var parent = new WorkItemMock(repository, runtime);
            parent.Id = 2;
            parent.TypeName = "Use Case";

            var firstChild = new WorkItemMock(repository, runtime);
            firstChild.Id = 3;
            firstChild.TypeName = "Task";
            var secondChild = new WorkItemMock(repository, runtime);
            secondChild.Id = 4;
            secondChild.TypeName = "Task";

            var tc1 = new WorkItemMock(repository, runtime);
            tc1.Id = 21;
            tc1.TypeName = "Test Case";
            tc1["Microsoft.VSTS.Scheduling.RemainingWork"] = 10.0;
            var tc2 = new WorkItemMock(repository, runtime);
            tc2.Id = 22;
            tc2.TypeName = "Test Case";
            tc2["Microsoft.VSTS.Scheduling.RemainingWork"] = 30.0;

            firstChild.WorkItemLinksImpl.Add(new WorkItemLinkMock(WorkItemImplementationBase.ParentRelationship, parent.Id, repository));
            secondChild.WorkItemLinksImpl.Add(new WorkItemLinkMock(WorkItemImplementationBase.ParentRelationship, parent.Id, repository));
            parent.WorkItemLinksImpl.Add(new WorkItemLinkMock(WorkItemImplementationBase.ParentRelationship, grandParent.Id, repository));

            grandParent.WorkItemLinksImpl.Add(new WorkItemLinkMock(WorkItemImplementationBase.ChildRelationship, parent.Id, repository));
            parent.WorkItemLinksImpl.Add(new WorkItemLinkMock(WorkItemImplementationBase.ChildRelationship, firstChild.Id, repository));
            parent.WorkItemLinksImpl.Add(new WorkItemLinkMock(WorkItemImplementationBase.ChildRelationship, secondChild.Id, repository));

            // Tested By
            grandParent.WorkItemLinksImpl.Add(new WorkItemLinkMock("Microsoft.VSTS.Common.TestedBy-Forward", tc1.Id, repository));

            // Tests
            tc1.WorkItemLinksImpl.Add(new WorkItemLinkMock("Microsoft.VSTS.Common.TestedBy-Reverse", grandParent.Id, repository));

            // Tested By
            grandParent.WorkItemLinksImpl.Add(new WorkItemLinkMock("Microsoft.VSTS.Common.TestedBy-Forward", tc2.Id, repository));

            // Tests
            tc2.WorkItemLinksImpl.Add(new WorkItemLinkMock("Microsoft.VSTS.Common.TestedBy-Reverse", grandParent.Id, repository));

            repository.SetWorkItems(new[] { grandParent, parent, firstChild, secondChild, tc1, tc2 });

            startPoint = grandParent;
            return repository;
        }

        [TestMethod]
        public void FluentNavigation_succeedes()
        {
            IWorkItem startPoint;
            MakeRepository(out startPoint);
            Substitute.For<ILogEvents>();

            var searchResult = startPoint
                .WhereTypeIs("Task")
                .AtMost(2)
                .FollowingLinks("*")
                .ToArray();

            Assert.AreEqual(2, searchResult.Length);
            Assert.AreEqual(3, searchResult[0].Id);
            Assert.AreEqual(4, searchResult[1].Id);
        }

        [TestMethod]
        public void FluentNavigation_CSharp_script_succeedes()
        {
            string script = @"
var searchResult = self
    .WhereTypeIs(""Task"")
    .AtMost(2)
    .FollowingLinks(""*"");
return searchResult;
";
            IWorkItem startPoint;
            var repository = MakeRepository(out startPoint);
            var logger = Substitute.For<ILogEvents>();
            repository.Logger = logger;
            var library = Substitute.For<IScriptLibrary>();
            var engine = new CSharpScriptEngine(logger, false, library);
            engine.LoadAndRun("test", script, startPoint, repository);

            var expected = new FluentQuery(startPoint);
            expected.WorkItemType = "Task";
            expected.Levels = 2;
            expected.LinkType = "*";
            logger.Received().ResultsFromScriptRun("test", expected);
        }

        [TestMethod]
        public void FluentNavigation_FollowingLinks_two_times()
        {
            string script = @"
var requirements = self.FollowingLinks(""Microsoft.VSTS.Common.TestedBy-Reverse"");
foreach(var req in requirements) {
  logger.Log(""Requirement #{0}"", req.Id);
  var testCases = req.FollowingLinks(""Microsoft.VSTS.Common.TestedBy-Forward"");
  double remaining = testCases.Sum(tc => tc.GetField(""Microsoft.VSTS.Scheduling.RemainingWork"", 0.0));
  req[""Custom.RemainingWork""] = req.GetField(""Microsoft.VSTS.Scheduling.RemainingWork"", 0.0) + remaining;
}
";
            IWorkItem startPoint;
            var repository = MakeRepository(out startPoint);
            var logger = new DebugEventLogger();
            repository.Logger = logger;
            var tc2 = repository.GetWorkItem(22);

            var library = Substitute.For<IScriptLibrary>();
            var engine = new CSharpScriptEngine(logger, true, library);
            engine.LoadAndRun("test", script, tc2, repository);

            Assert.AreEqual(42.0, startPoint["Custom.RemainingWork"]);
        }

        [TestMethod]
        public void TransitionState_InProgress_to_Done_succeeded()
        {
            var logger = new DebugEventLogger();
            var settings = TestHelpers.LoadConfigFromResourceFile("NewObjects.policies", logger);
            var repository = new WorkItemRepositoryMock();
            System.Func<IRuntimeContext, IScriptLibrary> scriptLibraryBuilder = (x) => Substitute.For<IScriptLibrary>();
            var context = Substitute.For<IRequestContext>();
            context.GetProjectCollectionUri().Returns(
                new System.Uri("http://localhost:8080/tfs/DefaultCollection"));
            var runtime = RuntimeContext.MakeRuntimeContext("settingsPath", settings, context, logger, (c) => repository, scriptLibraryBuilder);

            var workItem = new WorkItemMock(repository, runtime);
            workItem.Id = 42;
            workItem.TypeName = "Task";
            workItem.Type = new WorkItemTypeMock() { Name = "Task" };
            workItem.Fields["System.State"].Value = "In Progress";
            repository.SetWorkItems(new[] { workItem });
            string targetState = "Done";

            workItem.TransitionToState(targetState, "test");

            Assert.AreEqual(targetState, workItem.Fields["System.State"].Value);
            Assert.IsTrue(workItem.InternalWasSaveCalled);
        }

        [TestMethod]
        public void TransitionState_New_to_Done_succeeded_via_InProgress()
        {
            var logger = new DebugEventLogger();
            var settings = TestHelpers.LoadConfigFromResourceFile("NewObjects.policies", logger);
            var repository = new WorkItemRepositoryMock();
            System.Func<IRuntimeContext, IScriptLibrary> scriptLibraryBuilder = (x) => Substitute.For<IScriptLibrary>();
            var context = Substitute.For<IRequestContext>();
            context.GetProjectCollectionUri().Returns(
                new System.Uri("http://localhost:8080/tfs/DefaultCollection"));
            var runtime = RuntimeContext.MakeRuntimeContext("settingsPath", settings, context, logger, (c) => repository, scriptLibraryBuilder);

            var workItem = new WorkItemMock(repository, runtime);
            var workItemType = new WorkItemTypeMock()
            {
                Name = "Task",
                DocumentContent = TestHelpers.LoadTextFromEmbeddedResource("task.xml")
            };
            workItem.Id = 42;
            workItem.Type = workItemType;

            FieldMock mockedField = new FieldMock(workItem, "System.State");
            workItem.Fields[mockedField.Name] = mockedField;
            mockedField.OriginalValue = string.Empty;
            workItem.Fields["System.State"].Value = workItem.Fields["System.State"].OriginalValue;
            mockedField.Status = Microsoft.TeamFoundation.WorkItemTracking.Client.FieldStatus.InvalidValueNotInOtherField;
            repository.SetWorkItems(new[] { workItem });
            string targetState = "Done";

            workItem.TransitionToState(targetState, "test");

            Assert.AreEqual(targetState, workItem.Fields["System.State"].Value);
            Assert.AreEqual(2, workItem.InternalSaveCount);
        }

        [TestMethod]
        public void TransitionState_to_non_existing()
        {
            var logger = new DebugEventLogger();
            var settings = TestHelpers.LoadConfigFromResourceFile("NewObjects.policies", logger);
            var repository = new WorkItemRepositoryMock();
            System.Func<IRuntimeContext, IScriptLibrary> scriptLibraryBuilder = (x) => Substitute.For<IScriptLibrary>();
            var context = Substitute.For<IRequestContext>();
            context.GetProjectCollectionUri().Returns(
                new System.Uri("http://localhost:8080/tfs/DefaultCollection"));
            var runtime = RuntimeContext.MakeRuntimeContext("settingsPath", settings, context, logger, (c) => repository, scriptLibraryBuilder);

            var workItem = new WorkItemMock(repository, runtime);
            var workItemType = new WorkItemTypeMock()
            {
                Name = "Task",
                DocumentContent = TestHelpers.LoadTextFromEmbeddedResource("task.xml")
            };
            workItem.Id = 42;
            workItem.Type = workItemType;
            workItem.TypeName = workItemType.Name;

            FieldMock mockedField = new FieldMock(workItem, "System.State");
            mockedField.Value = string.Empty;
            mockedField.OriginalValue = string.Empty;
            mockedField.Status = Microsoft.TeamFoundation.WorkItemTracking.Client.FieldStatus.InvalidValueNotInOtherField;
            workItem.Fields[mockedField.Name] = mockedField;

            repository.SetWorkItems(new[] { workItem });
            string targetState = "DoesNotExists";

            workItem.TransitionToState(targetState, "test");

            Assert.AreNotEqual(targetState, workItem.Fields["System.State"].Value);
            Assert.AreEqual(workItem.Fields["System.State"].OriginalValue, workItem.Fields["System.State"].Value);
            Assert.IsFalse(workItem.InternalWasSaveCalled);
        }

        [TestMethod]
        public void TransitionStateCSharp_New_to_Done_succeeded_via_InProgress()
        {
            var logger = new DebugEventLogger();
            var settings = TestHelpers.LoadConfigFromResourceFile("NewObjects.policies", logger);
            var repository = new WorkItemRepositoryMock();
            System.Func<IRuntimeContext, IScriptLibrary> scriptLibraryBuilder = (x) => Substitute.For<IScriptLibrary>();
            var context = Substitute.For<IRequestContext>();
            context.GetProjectCollectionUri().Returns(
                new System.Uri("http://localhost:8080/tfs/DefaultCollection"));
            var runtime = RuntimeContext.MakeRuntimeContext("settingsPath", settings, context, logger, (c) => repository, scriptLibraryBuilder);

            string script = @"
self.TransitionToState(""Done"", ""script test"");
";

            repository.Logger = logger;
            var workItem = new WorkItemMock(repository, runtime);
            var workItemType = new WorkItemTypeMock()
            {
                Name = "Task",
                DocumentContent = TestHelpers.LoadTextFromEmbeddedResource("task.xml")
            };
            workItem.Id = 42;
            workItem.Type = workItemType;
            workItem.TypeName = workItemType.Name;

            FieldMock mockedField = new FieldMock(workItem, "System.State");
            workItem.Fields[mockedField.Name] = mockedField;
            mockedField.OriginalValue = string.Empty;
            mockedField.Value = mockedField.OriginalValue;
            mockedField.Status = Microsoft.TeamFoundation.WorkItemTracking.Client.FieldStatus.InvalidValueNotInOtherField;

            repository.SetWorkItems(new[] { workItem });

            var library = Substitute.For<IScriptLibrary>();
            var engine = new CSharpScriptEngine(logger, false, library);
            engine.LoadAndRun("test", script, workItem, repository);

            Assert.AreEqual("Done", workItem.Fields["System.State"].Value);
            Assert.AreEqual(2, workItem.InternalSaveCount);
        }
    }
}

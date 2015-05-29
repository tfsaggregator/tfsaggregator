using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitTests.Core
{
    using Aggregator.Core;
    using Aggregator.Core.Navigation;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using NSubstitute;
    using UnitTests.Core.Mock;

    [TestClass]
    public class Navigation
    {
        private static IWorkItemRepository MakeRepository(out IWorkItem startPoint)
        {
            var repository = new WorkItemRepositoryMock();

            var grandParent = new WorkItemMock(repository);
            grandParent.Id = 1;
            grandParent.TypeName = "Feature";

            var parent = new WorkItemMock(repository);
            parent.Id = 2;
            parent.TypeName = "Use Case";

            var firstChild = new WorkItemMock(repository);
            firstChild.Id = 3;
            firstChild.TypeName = "Task";
            var secondChild = new WorkItemMock(repository);
            secondChild.Id = 4;
            secondChild.TypeName = "Task";

            firstChild.WorkItemLinks.Add(new WorkItemLinkMock(WorkItemImplementationBase.ParentRelationship, parent.Id, repository));
            secondChild.WorkItemLinks.Add(new WorkItemLinkMock(WorkItemImplementationBase.ParentRelationship, parent.Id, repository));
            parent.WorkItemLinks.Add(new WorkItemLinkMock(WorkItemImplementationBase.ParentRelationship, grandParent.Id, repository));

            grandParent.WorkItemLinks.Add(new WorkItemLinkMock(WorkItemImplementationBase.ChildRelationship, parent.Id, repository));
            parent.WorkItemLinks.Add(new WorkItemLinkMock(WorkItemImplementationBase.ChildRelationship, firstChild.Id, repository));
            parent.WorkItemLinks.Add(new WorkItemLinkMock(WorkItemImplementationBase.ChildRelationship, secondChild.Id, repository));

            repository.SetWorkItems(new[] { grandParent, parent, firstChild, secondChild });

            startPoint = grandParent;
            return repository;
        }

        [TestMethod]
        public void FluentNavigation_succeedes()
        {
            IWorkItem startPoint;
            var repository = MakeRepository(out startPoint);
            var logger = Substitute.For<ILogEvents>();

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

            var engine = new CSharpScriptEngine(repository, logger);
            engine.LoadAndRun("test", script, startPoint);

            var expected = new FluentQuery(startPoint);
            expected.WorkItemType = "Task";
            expected.Levels = 2;
            expected.LinkType = "*";
            logger.Received().ResultsFromScriptRun("test", expected);
        }

        [TestMethod]
        public void TransitionState_InProgress_to_Done_succeeded()
        {
            var repository = new WorkItemRepositoryMock();
            repository.Logger = Substitute.For<ILogEvents>();
            var workItem = new WorkItemMock(repository);
            workItem.Id = 42;
            workItem.TypeName = "Task";
            workItem.Fields["State"].Value = "In Progress";
            repository.SetWorkItems(new[] { workItem });
            string targetState = "Done";

            workItem.TransitionToState(targetState, "test");

            Assert.AreEqual(targetState, workItem.Fields["State"].Value);
            Assert.IsTrue(workItem._SaveCalled);
        }

        [TestMethod]
        public void TransitionState_New_to_Done_succeeded_via_InProgress()
        {
            var repository = new WorkItemRepositoryMock();
            repository.Logger = Substitute.For<ILogEvents>();
            var workItem = new WorkItemMock(repository);
            var workItemType = new WorkItemTypeMock() {
                Name = "Task",
                DocumentContent = TestHelpers.LoadTextFromEmbeddedResource("task.xml")
            };
            workItem.Id = 42;
            workItem.Type = workItemType;
            workItem.TypeName = workItemType.Name;
            ((FieldMock)workItem.Fields["State"]).OriginalValue = "";
            workItem.Fields["State"].Value = workItem.Fields["State"].OriginalValue;
            ((FieldMock)workItem.Fields["State"]).Status = Microsoft.TeamFoundation.WorkItemTracking.Client.FieldStatus.InvalidValueNotInOtherField;
            repository.SetWorkItems(new[] { workItem });
            string targetState = "Done";

            workItem.TransitionToState(targetState, "test");

            Assert.AreEqual(targetState, workItem.Fields["State"].Value);
            Assert.AreEqual(2, workItem._SaveCount);
        }

        [TestMethod]
        public void TransitionState_to_non_existing()
        {
            var repository = new WorkItemRepositoryMock();
            repository.Logger = Substitute.For<ILogEvents>();
            var workItem = new WorkItemMock(repository);
            var workItemType = new WorkItemTypeMock()
            {
                Name = "Task",
                DocumentContent = TestHelpers.LoadTextFromEmbeddedResource("task.xml")
            };
            workItem.Id = 42;
            workItem.Type = workItemType;
            workItem.TypeName = workItemType.Name;
            workItem.Fields["State"].Value = "";
            ((FieldMock)workItem.Fields["State"]).OriginalValue = "";
            ((FieldMock)workItem.Fields["State"]).Status = Microsoft.TeamFoundation.WorkItemTracking.Client.FieldStatus.InvalidValueNotInOtherField;
            repository.SetWorkItems(new[] { workItem });
            string targetState = "DoesNotExists";

            workItem.TransitionToState(targetState, "test");

            Assert.AreNotEqual(targetState, workItem.Fields["State"].Value);
            Assert.AreEqual(workItem.Fields["State"].OriginalValue, workItem.Fields["State"].Value);
            Assert.IsFalse(workItem._SaveCalled);
        }

        [TestMethod]
        public void TransitionStateCSharp_New_to_Done_succeeded_via_InProgress()
        {
            string script = @"
self.TransitionToState(""Done"", ""script test"");
";
            var repository = new WorkItemRepositoryMock();
            var logger = Substitute.For<ILogEvents>();
            repository.Logger = logger;
            var workItem = new WorkItemMock(repository);
            var workItemType = new WorkItemTypeMock()
            {
                Name = "Task",
                DocumentContent = TestHelpers.LoadTextFromEmbeddedResource("task.xml")
            };
            workItem.Id = 42;
            workItem.Type = workItemType;
            workItem.TypeName = workItemType.Name;
            ((FieldMock)workItem.Fields["State"]).OriginalValue = "";
            workItem.Fields["State"].Value = workItem.Fields["State"].OriginalValue;
            ((FieldMock)workItem.Fields["State"]).Status = Microsoft.TeamFoundation.WorkItemTracking.Client.FieldStatus.InvalidValueNotInOtherField;
            repository.SetWorkItems(new[] { workItem });

            var engine = new CSharpScriptEngine(repository, logger);
            engine.LoadAndRun("test", script, workItem);

            Assert.AreEqual("Done", workItem.Fields["State"].Value);
            Assert.AreEqual(2, workItem._SaveCount);
        }
    }
}

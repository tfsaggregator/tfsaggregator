using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            var repository = new WorkItemRepositoryMock();

            var grandParent = new WorkItemMock(repository, null);
            grandParent.Id = 1;
            grandParent.TypeName = "Feature";

            var parent = new WorkItemMock(repository, null);
            parent.Id = 2;
            parent.TypeName = "Use Case";

            var firstChild = new WorkItemMock(repository, null);
            firstChild.Id = 3;
            firstChild.TypeName = "Task";
            var secondChild = new WorkItemMock(repository, null);
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

            var engine = new CSharpScriptEngine(logger, false);
            engine.LoadAndRun("test", script, startPoint, repository);

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
            var workItem = new WorkItemMock(repository, null);
            workItem.Id = 42;
            workItem.TypeName = "Task";
            workItem.Fields["State"].Value = "In Progress";
            repository.SetWorkItems(new[] { workItem });
            string targetState = "Done";

            workItem.TransitionToState(targetState, "test");

            Assert.AreEqual(targetState, workItem.Fields["State"].Value);
            Assert.IsTrue(workItem.InternalWasSaveCalled);
        }

        [TestMethod]
        public void TransitionState_New_to_Done_succeeded_via_InProgress()
        {
            var repository = new WorkItemRepositoryMock();
            repository.Logger = Substitute.For<ILogEvents>();
            var workItem = new WorkItemMock(repository, null);
            var workItemType = new WorkItemTypeMock()
            {
                Name = "Task",
                DocumentContent = TestHelpers.LoadTextFromEmbeddedResource("task.xml")
            };
            workItem.Id = 42;
            workItem.Type = workItemType;
            workItem.TypeName = workItemType.Name;
            FieldMock mockedField = new FieldMock(workItem, "State");
            workItem.Fields[mockedField.Name] = mockedField;
            mockedField.OriginalValue = string.Empty;
            workItem.Fields["State"].Value = workItem.Fields["State"].OriginalValue;
            mockedField.Status = Microsoft.TeamFoundation.WorkItemTracking.Client.FieldStatus.InvalidValueNotInOtherField;
            repository.SetWorkItems(new[] { workItem });
            string targetState = "Done";

            workItem.TransitionToState(targetState, "test");

            Assert.AreEqual(targetState, workItem.Fields["State"].Value);
            Assert.AreEqual(2, workItem.InternalSaveCount);
        }

        [TestMethod]
        public void TransitionState_to_non_existing()
        {
            var repository = new WorkItemRepositoryMock();
            repository.Logger = Substitute.For<ILogEvents>();
            var workItem = new WorkItemMock(repository, null);
            var workItemType = new WorkItemTypeMock()
            {
                Name = "Task",
                DocumentContent = TestHelpers.LoadTextFromEmbeddedResource("task.xml")
            };
            workItem.Id = 42;
            workItem.Type = workItemType;
            workItem.TypeName = workItemType.Name;

            FieldMock mockedField = new FieldMock(workItem, "State");
            mockedField.Value = string.Empty;
            mockedField.OriginalValue = string.Empty;
            mockedField.Status = Microsoft.TeamFoundation.WorkItemTracking.Client.FieldStatus.InvalidValueNotInOtherField;
            workItem.Fields[mockedField.Name] = mockedField;

            repository.SetWorkItems(new[] { workItem });
            string targetState = "DoesNotExists";

            workItem.TransitionToState(targetState, "test");

            Assert.AreNotEqual(targetState, workItem.Fields["State"].Value);
            Assert.AreEqual(workItem.Fields["State"].OriginalValue, workItem.Fields["State"].Value);
            Assert.IsFalse(workItem.InternalWasSaveCalled);
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
            var workItem = new WorkItemMock(repository, null);
            var workItemType = new WorkItemTypeMock()
            {
                Name = "Task",
                DocumentContent = TestHelpers.LoadTextFromEmbeddedResource("task.xml")
            };
            workItem.Id = 42;
            workItem.Type = workItemType;
            workItem.TypeName = workItemType.Name;

            FieldMock mockedField = new FieldMock(workItem, "State");
            workItem.Fields[mockedField.Name] = mockedField;
            mockedField.OriginalValue = string.Empty;
            mockedField.Value = mockedField.OriginalValue;
            mockedField.Status = Microsoft.TeamFoundation.WorkItemTracking.Client.FieldStatus.InvalidValueNotInOtherField;

            repository.SetWorkItems(new[] { workItem });

            var engine = new CSharpScriptEngine(logger, false);
            engine.LoadAndRun("test", script, workItem, repository);

            Assert.AreEqual("Done", workItem.Fields["State"].Value);
            Assert.AreEqual(2, workItem.InternalSaveCount);
        }
    }
}

using System.Linq;

using Aggregator.Core;
using Aggregator.Core.Context;
using Aggregator.Core.Extensions;
using Aggregator.Core.Interfaces;

using Microsoft.TeamFoundation.Framework.Server;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using NSubstitute;

using UnitTests.Core.Mock;

namespace UnitTests.Core
{
    [TestClass]
    public class ItemCreationTests
    {
        [TestMethod]
        public void WorkItemLink_addNew_succeeds()
        {
            var logger = new DebugEventLogger();
            var settings = TestHelpers.LoadConfigFromResourceFile("NewObjects.policies", logger);

            var repository = new WorkItemRepositoryMock();

            var parent = new WorkItemMock(repository);
            parent.Id = 1;
            parent.TypeName = "Use Case";
            parent["Title"] = "UC";

            var child = new WorkItemMock(repository);
            child.Id = 2;
            child.TypeName = "Task";
            child["Title"] = "TSK";

            repository.SetWorkItems(new[] { parent, child });

            var context = Substitute.For<IRequestContext>();
            var runtime = RuntimeContext.MakeRuntimeContext("settingsPath", settings, context, logger);
            using (var processor = new EventProcessor(repository, runtime))
            {
                var notification = Substitute.For<INotification>();
                notification.WorkItemId.Returns(2);

                var result = processor.ProcessEvent(context, notification);

                Assert.AreEqual(0, result.ExceptionProperties.Count());
                Assert.IsTrue(child._SaveCalled);
                Assert.AreEqual(EventNotificationStatus.ActionPermitted, result.NotificationStatus);
            }
        }

        [TestMethod]
        public void WorkItemLink_addExisting_noop()
        {
            var logger = new DebugEventLogger();
            var settings = TestHelpers.LoadConfigFromResourceFile("NewObjects.policies", logger);

            var repository = new WorkItemRepositoryMock();

            var parent = new WorkItemMock(repository);
            parent.Id = 1;
            parent.TypeName = "Use Case";
            parent["Title"] = "UC";

            var child = new WorkItemMock(repository);
            child.Id = 2;
            child.TypeName = "Task";
            child["Title"] = "TSK";

            child.WorkItemLinks.Add(new WorkItemLinkMock(WorkItemImplementationBase.ParentRelationship, parent.Id, repository));
            repository.SetWorkItems(new[] { parent, child });

            var context = Substitute.For<IRequestContext>();
            var runtime = RuntimeContext.MakeRuntimeContext("settingsPath", settings, context, logger);
            using (var processor = new EventProcessor(repository, runtime))
            {
                var notification = Substitute.For<INotification>();
                notification.WorkItemId.Returns(2);

                var result = processor.ProcessEvent(context, notification);

                Assert.AreEqual(0, result.ExceptionProperties.Count());
                Assert.IsFalse(child._SaveCalled);
                Assert.IsFalse(parent._SaveCalled);
                Assert.AreEqual(EventNotificationStatus.ActionPermitted, result.NotificationStatus);
            }
        }

        [TestMethod]
        public void WorkItem_addNew_succeeds()
        {
            var logger = new DebugEventLogger();
            var settings = TestHelpers.LoadConfigFromResourceFile("NewObjects.policies", logger);

            var repository = new WorkItemRepositoryMock();

            var parent = new WorkItemMock(repository);
            parent.Id = 1;
            parent.TypeName = "Bug";
            parent["Title"] = "My bug #1";

            repository.SetWorkItems(new[] { parent });

            var context = Substitute.For<IRequestContext>();
            var runtime = RuntimeContext.MakeRuntimeContext("settingsPath", settings, context, logger);
            using (var processor = new EventProcessor(repository, runtime))
            {
                var notification = Substitute.For<INotification>();
                notification.WorkItemId.Returns(1);

                var result = processor.ProcessEvent(context, notification);

                Assert.AreEqual(0, result.ExceptionProperties.Count());
                Assert.AreEqual(2, repository.LoadedWorkItems.Count);
                Assert.IsTrue(parent._SaveCalled);
                Assert.IsTrue(parent.HasChildren());
            }
        }
    }
}
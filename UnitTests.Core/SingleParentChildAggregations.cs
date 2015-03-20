using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using System.Linq;
using TFS = Microsoft.TeamFoundation.WorkItemTracking.Client;

namespace UnitTests.Core
{
    using Microsoft.TeamFoundation.Framework.Server;
    using Aggregator.Core;
    using UnitTests.Core.Mock;
    using System.Collections.ObjectModel;
    using System.Collections.Generic;

    [TestClass]
    public class SingleParentChildAggregations
    {
        private IWorkItemRepository repository;
        private IWorkItem workItem;

        private IWorkItemRepository SetupFakeRepository()
        {
            repository = Substitute.For<IWorkItemRepository>();

            workItem = Substitute.For<IWorkItem>();
            workItem.Id.Returns(1);
            workItem.TypeName.Returns("Task");
            workItem.Fields["Estimated Work"].Value = 0.0D;
            workItem.Fields["Estimated Dev Work"].Value.Returns(1.0D);
            workItem.Fields["Estimated Test Work"].Value.Returns(2.0D);
            workItem.IsValid().Returns(true);

            repository.GetWorkItem(1).Returns(workItem);
            repository.LoadedWorkItems.Returns(new ReadOnlyCollection<IWorkItem>(new List<IWorkItem>() { workItem }));

            return repository;
        }

        private IWorkItemRepository SetupFakeRepository_Short()
        {
            repository = Substitute.For<IWorkItemRepository>();

            workItem = Substitute.For<IWorkItem>();
            workItem.Id.Returns(1);
            workItem.TypeName.Returns("Task");
            workItem["Estimated Dev Work"].Returns(1.0D);
            workItem["Estimated Test Work"].Returns(2.0D);
            workItem["Finish Date"].Returns(new DateTime(2010,1,1));
            workItem.IsValid().Returns(true);

            repository.GetWorkItem(1).Returns(workItem);
            repository.LoadedWorkItems.Returns(new ReadOnlyCollection<IWorkItem>(new List<IWorkItem>() { workItem }));

            return repository;
        }

        [TestMethod]
        public void Should_aggregate_a_numeric_field()
        {
            var settings = TestHelpers.LoadConfigFromResourceFile("SumFieldsOnSingleWorkItem.policies");
            var repository = SetupFakeRepository();
            var logger = Substitute.For<ILogEvents>();
            var processor = new EventProcessor(repository, logger, settings);
            var context = Substitute.For<IRequestContext>();
            var notification = Substitute.For<INotification>();
            notification.WorkItemId.Returns(1);

            var result = processor.ProcessEvent(context, notification);

            Assert.AreEqual(0, result.ExceptionProperties.Count());
            workItem.Received().Save();
            Assert.AreEqual(3.0D, workItem.Fields["Estimated Work"].Value);
            Assert.AreEqual(EventNotificationStatus.ActionPermitted, result.NotificationStatus);
        }

        [TestMethod]
        public void Should_aggregate_a_numeric_field_short()
        {
            var settings = TestHelpers.LoadConfigFromResourceFile("SumFieldsOnSingleWorkItem-Short.policies");
            var repository = SetupFakeRepository_Short();
            var logger = Substitute.For<ILogEvents>();
            var processor = new EventProcessor(repository, logger, settings);
            var context = Substitute.For<IRequestContext>();
            var notification = Substitute.For<INotification>();
            notification.WorkItemId.Returns(1);

            var result = processor.ProcessEvent(context, notification);

            Assert.AreEqual(0, result.ExceptionProperties.Count());
            workItem.Received().Save();
            Assert.AreEqual(3.0D, workItem["Estimated Work"]);
            Assert.AreEqual(EventNotificationStatus.ActionPermitted, result.NotificationStatus);
        }

        [TestMethod]
        public void Should_aggregate_a_numeric_field_VB()
        {
            var settings = TestHelpers.LoadConfigFromResourceFile("SumFieldsOnSingleWorkItemVB.policies");
            var repository = SetupFakeRepository_Short();
            var logger = Substitute.For<ILogEvents>();
            var processor = new EventProcessor(repository, logger, settings);
            var context = Substitute.For<IRequestContext>();
            var notification = Substitute.For<INotification>();
            notification.WorkItemId.Returns(1);

            var result = processor.ProcessEvent(context, notification);

            Assert.AreEqual(0, result.ExceptionProperties.Count());
            workItem.Received().Save();
            Assert.AreEqual(3.0D, workItem["Estimated Work"]);
            Assert.AreEqual(EventNotificationStatus.ActionPermitted, result.NotificationStatus);
        }


        [TestMethod]
        public void Should_aggregate_to_parent()
        {
            var settings = TestHelpers.LoadConfigFromResourceFile("Rollup.policies");
            
            var repository = new WorkItemRepositoryMock();

            var grandParent = new WorkItemMock();
            grandParent.Id = 1;
            grandParent.TypeName = "Feature";
            grandParent["Dev Estimate"] = 0.0D;
            grandParent["Test Estimate"] = 0.0D;

            var parent = new WorkItemMock();
            parent.Id = 2;
            parent.TypeName = "Use Case";
            parent["Total Work Remaining"] = 3.0D;
            parent["Total Estimate"] = 4.0D;

            var workItem = new WorkItemMock();
            workItem.Id = 3;
            workItem.TypeName = "Task";
            workItem["Estimated Dev Work"] = 10.0D;
            workItem["Estimated Test Work"] = 20.0D;
            workItem["Remaining Dev Work"] = 1.0D;
            workItem["Remaining Test Work"] = 2.0D;
            workItem["Finish Date"] = new DateTime(2015,1,1);

            workItem.Parent = new WorkItemLazyReference(parent.Id, repository);
            parent.Parent = new WorkItemLazyReference(grandParent.Id, repository);
            repository.SetWorkItems(new[] { grandParent, parent, workItem });

            var logger = Substitute.For<ILogEvents>();
            var processor = new EventProcessor(repository, logger, settings);
            var context = Substitute.For<IRequestContext>();
            var notification = Substitute.For<INotification>();
            notification.WorkItemId.Returns(3);

            var result = processor.ProcessEvent(context, notification);

            Assert.AreEqual(0, result.ExceptionProperties.Count());
            Assert.IsTrue(workItem._SaveCalled);
            Assert.IsTrue(parent._SaveCalled);
            Assert.IsFalse(grandParent._SaveCalled);
            Assert.AreEqual(3.0D, parent["Total Work Remaining"]);
            Assert.AreEqual(30.0D, parent["Total Estimate"]);
            Assert.AreEqual(EventNotificationStatus.ActionPermitted, result.NotificationStatus);
        }
    }
}

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using System.Linq;
using TFS = Microsoft.TeamFoundation.WorkItemTracking.Client;

namespace UnitTests.Core
{
    using Microsoft.TeamFoundation.Framework.Server;
    using Aggregator.Core;

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

            return repository;
        }

        [TestMethod]
        public void Should_aggregate_a_numeric_field()
        {
            var settings = TestHelpers.LoadConfigFromResourceFile("SumFieldsOnSingleWorkItem.policies");
            var repository = SetupFakeRepository();
            var logger = Substitute.For<ILogEvents>();
            var processor = new EventProcessor(repository, logger);
            var context = Substitute.For<IRequestContext>();
            var notification = Substitute.For<INotification>();
            notification.WorkItemId.Returns(1);

            var result = processor.ProcessEvent(context, notification, settings);

            Assert.AreEqual(0, result.ExceptionProperties.Count());
            workItem.Received().Save();
            Assert.AreEqual(3.0D, workItem.Fields["Estimated Work"].Value);
            Assert.AreEqual(EventNotificationStatus.ActionPermitted, result.NotificationStatus);
        }
    }
}

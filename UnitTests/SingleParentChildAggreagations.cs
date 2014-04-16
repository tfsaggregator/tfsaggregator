using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TFSAggregator;
using TFSAggregator.TfsFacade;
using Microsoft.TeamFoundation.Framework.Server;
using NSubstitute;
using TFSAggregator.TfsSpecific;

namespace UnitTests
{
    [TestClass]
    public class SingleParentChildAggregations
    {
        private IWorkItemRepository SetupFakeRepository()
        {
            var repository = Substitute.For<IWorkItemRepository>();

            var workItem = Substitute.For<IWorkItem>();
            workItem.Id.Returns(1);
            workItem.TypeName.Returns("Task");

            repository.GetWorkItem(1).Returns(workItem);

            return repository;
        }

        [TestMethod]
        public void Should_aggregate_a_numeric_field()
        {
            TestHelpers.SetConfigResourceFile("SumFieldsOnSingleWorkItem.xml");

            var repository = SetupFakeRepository();
            var processor = new EventProcessor(repository);

            var context = Substitute.For<IRequestContext>();
            var notification = Substitute.For<INotification>();
            notification.WorkItemId.Returns(1);

            var result = processor.ProcessEvent(context, notification);
            Assert.AreEqual(result.NotificationStatus, EventNotificationStatus.ActionApproved);
        }
    }
}

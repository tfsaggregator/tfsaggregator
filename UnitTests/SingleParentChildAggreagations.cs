using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TFSAggregator;
using TFSAggregator.TfSFacade;
using Microsoft.TeamFoundation.Framework.Server;

namespace UnitTests
{
    [TestClass]
    public class SingleParentChildAggregations
    {
        [TestMethod]
        public void Should_aggregate_a_numeric_field()
        {
            TestHelpers.SetConfigResourceFile("SumFieldsOnSingleWorkItem.xml");

            var processor = new EventProcessor();
            var result = processor.ProcessEvent(null, null);
            Assert.AreEqual(result.NotificationStatus, EventNotificationStatus.ActionApproved);
        }
    }
}

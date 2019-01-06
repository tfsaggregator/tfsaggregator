using System;
using Aggregator.Core.Interfaces;
using Aggregator.WebHooks.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTest.WebHooks
{
    [TestClass]
    [DeploymentItem("PostData", "PostData")]
    public class ParsePayloadˆVSTS : PayloadBase
    {
        [TestMethod]
        public void Created_All_None_NoneˆSucceeds()
        {
            var payload = GetDataFileMatchingCallerName();

            var request = WorkItemRequest.Parse(payload);

            Assert.IsNotNull(request);
            Assert.IsNotNull(request);
            Assert.IsTrue(request.IsValid);
            Assert.AreEqual(ChangeTypes.New, request.ChangeType);
            Assert.AreEqual("1fe04a90-6116-437a-a208-f64a7711e8ff", request.CollectionId);
            Assert.AreEqual("https://giuliovaad.visualstudio.com/", request.TfsCollectionUri);
            Assert.AreEqual(14, request.WorkItemId);
            Assert.AreEqual("WorkItemTracking", request.TeamProject);
        }

        [TestMethod]
        public void Updated_All_None_NoneˆSucceeds()
        {
            var payload = GetDataFileMatchingCallerName();

            var request = WorkItemRequest.Parse(payload);

            Assert.IsNotNull(request);
            Assert.IsTrue(request.IsValid);
            Assert.AreEqual(ChangeTypes.Change, request.ChangeType);
            Assert.AreEqual("1fe04a90-6116-437a-a208-f64a7711e8ff", request.CollectionId);
            Assert.AreEqual("https://giuliovaad.visualstudio.com/", request.TfsCollectionUri);
            Assert.AreEqual(12, request.WorkItemId);
            Assert.AreEqual("WorkItemTracking", request.TeamProject);
        }

        [TestMethod]
        public void Updated_Minimal_None_NoneˆFails()
        {
            var payload = GetDataFileMatchingCallerName();

            var request = WorkItemRequest.Parse(payload);

            Assert.IsNotNull(request);
            Assert.IsFalse(request.IsValid);
            Assert.AreEqual("TFS Aggregator requires 'All' for 'Resource details to send'.", request.Error);
        }
    }
}

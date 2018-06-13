using System;
using System.IO;
using System.Runtime.CompilerServices;
using Aggregator.Core.Interfaces;
using Aggregator.WebHooks.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;

namespace UnitTest.WebHooks
{
    [TestClass]
    [DeploymentItem("PostData", "PostData")]
    public class ParsePayloadˆTFS2017u31 : PayloadBase
    {
        [TestMethod]
        public void Created_All_None_NoneˆSucceeds()
        {
            var payload = GetDataFileMatchingCallerName();

            var request = WorkItemRequest.Parse(payload);

            Assert.IsNotNull(request);
            Assert.IsTrue(request.IsValid);
            Assert.AreEqual(ChangeTypes.New, request.ChangeType);
            Assert.AreEqual("fcb4f585-afdd-4dc8-bb1b-1d8d5d9d2ff2", request.CollectionId);
            Assert.AreEqual("https://win-ro13gv4kfhq/DefaultCollection/", request.TfsCollectionUri);
            Assert.AreEqual(2, request.WorkItemId);
            Assert.AreEqual("FirstProject", request.TeamProject);
        }

        [TestMethod]
        public void Updated_All_None_NoneˆSucceeds()
        {
            var payload = GetDataFileMatchingCallerName();

            var request = WorkItemRequest.Parse(payload);

            Assert.IsNotNull(request);
            Assert.IsTrue(request.IsValid);
            Assert.AreEqual(ChangeTypes.Change, request.ChangeType);
            Assert.AreEqual("fcb4f585-afdd-4dc8-bb1b-1d8d5d9d2ff2", request.CollectionId);
            Assert.AreEqual("https://win-ro13gv4kfhq/DefaultCollection/", request.TfsCollectionUri);
            Assert.AreEqual(1, request.WorkItemId);
            Assert.AreEqual("FirstProject", request.TeamProject);
        }

        [TestMethod]
        public void Deleted_All_None_NoneˆSucceeds()
        {
            var payload = GetDataFileMatchingCallerName();

            var request = WorkItemRequest.Parse(payload);

            Assert.IsNotNull(request);
            Assert.IsTrue(request.IsValid);
            Assert.AreEqual(ChangeTypes.Delete, request.ChangeType);
            Assert.AreEqual("fcb4f585-afdd-4dc8-bb1b-1d8d5d9d2ff2", request.CollectionId);
            Assert.AreEqual("https://win-ro13gv4kfhq/DefaultCollection/", request.TfsCollectionUri);
            Assert.AreEqual(2, request.WorkItemId);
            Assert.AreEqual("FirstProject", request.TeamProject);
        }

        [TestMethod]
        public void Restored_All_None_NoneˆSucceeds()
        {
            var payload = GetDataFileMatchingCallerName();

            var request = WorkItemRequest.Parse(payload);

            Assert.IsNotNull(request);
            Assert.IsTrue(request.IsValid);
            Assert.AreEqual(ChangeTypes.Restore, request.ChangeType);
            Assert.AreEqual("fcb4f585-afdd-4dc8-bb1b-1d8d5d9d2ff2", request.CollectionId);
            Assert.AreEqual("https://win-ro13gv4kfhq/DefaultCollection/", request.TfsCollectionUri);
            Assert.AreEqual(2, request.WorkItemId);
            Assert.AreEqual("FirstProject", request.TeamProject);
        }
    }
}

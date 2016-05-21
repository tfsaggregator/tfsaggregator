using System.Linq;

using Aggregator.Core;
using Aggregator.Core.Context;
using Aggregator.Core.Extensions;
using Aggregator.Core.Interfaces;

using Microsoft.TeamFoundation.Framework.Server;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using NSubstitute;

using UnitTests.Core.Mock;

namespace UnitTests.Core
{
    [TestClass]
    public class ItemDestroyTests
    {
        [TestMethod]
        public void WorkItemLink_RemoveExisting_succeeds()
        {
            var logger = new DebugEventLogger();
            var settings = TestHelpers.LoadConfigFromResourceFile("DeleteObjects.policies", logger);

            var repository = new WorkItemRepositoryMock();
            System.Func<IRuntimeContext, IScriptLibrary> scriptLibraryBuilder = (x) => Substitute.For<IScriptLibrary>();

            var context = Substitute.For<IRequestContext>();
            context.GetProjectCollectionUri().Returns(
                new System.Uri("http://localhost:8080/tfs/DefaultCollection"));
            var runtime = RuntimeContext.MakeRuntimeContext("settingsPath", settings, context, logger, (c) => repository, scriptLibraryBuilder);

            var parent = new WorkItemMock(repository, runtime);
            parent.Id = 1;
            parent.TypeName = "Use Case";
            parent["Title"] = "UC";

            var child = new WorkItemMock(repository, runtime);
            child.Id = 2;
            child.TypeName = "Task";
            child["Title"] = "TSK";

            parent.WorkItemLinks.Add(new WorkItemLinkMock(WorkItemImplementationBase.ChildRelationship, child.Id, repository));
            child.WorkItemLinks.Add(new WorkItemLinkMock(WorkItemImplementationBase.ParentRelationship, parent.Id, repository));

            repository.SetWorkItems(new[] { parent, child });

            using (var processor = new EventProcessor(runtime))
            {
                var notification = Substitute.For<INotification>();
                notification.WorkItemId.Returns(2);

                var result = processor.ProcessEvent(context, notification);

                Assert.AreEqual(0, result.ExceptionProperties.Count);
                Assert.AreEqual(0, child.WorkItemLinks.Count());
                Assert.AreEqual(EventNotificationStatus.ActionPermitted, result.NotificationStatus);
                Assert.IsTrue(child.InternalWasSaveCalled);
                Assert.IsTrue(parent.InternalWasSaveCalled);
            }
        }
    }
}
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
    public class GlobalListsTests
    {
        [TestMethod]
        public void GlobalList_get_Succeeded()
        {
            var repository = new WorkItemRepositoryMock();

            var gl = repository.GetGlobalList("Aggregator - UserParameters").ToArray();

            Assert.IsNotNull(gl);
            Assert.AreEqual(1, gl.Length);
            Assert.AreEqual("myParameter=30", gl[0]);
        }

        [TestMethod]
        public void GlobalList_UserParameterAddValue_Succeeded()
        {
            var logger = new DebugEventLogger();
            var settings = TestHelpers.LoadConfigFromResourceFile("UserParameters.policies", logger);

            var context = Substitute.For<IRequestContext>();
            context.GetProjectCollectionUri().Returns(
                new System.Uri("http://localhost:8080/tfs/DefaultCollection"));

            var repository = new WorkItemRepositoryMock();
            System.Func<IRuntimeContext, IScriptLibrary> scriptLibraryBuilder = (x) => Substitute.For<IScriptLibrary>();
            var runtime = RuntimeContext.MakeRuntimeContext(@"C:\GlobalList_UserParameterAddValue_Succeeded", settings, context, logger, (c) => repository, scriptLibraryBuilder);

            var workItem = new WorkItemMock(repository, runtime);
            workItem.Id = 1;
            workItem.TypeName = "Use Case";
            workItem["Title"] = "The car shall have a maximum speed of {myParameter} mph.";

            repository.SetWorkItems(new[] { workItem });

            using (var processor = new EventProcessor(runtime))
            {
                var notification = Substitute.For<INotification>();
                notification.WorkItemId.Returns(1);

                var result = processor.ProcessEvent(context, notification);

                Assert.AreEqual(0, result.ExceptionProperties.Count);
                Assert.IsTrue(workItem.InternalWasSaveCalled);
                Assert.AreEqual("The car shall have a maximum speed of {myParameter}(30) mph.", workItem.Fields["Title"].Value);
                Assert.AreEqual(EventNotificationStatus.ActionPermitted, result.NotificationStatus);
            }
        }

        [TestMethod]
        public void GlobalList_UserParameterReplaceExisting_Succeeded()
        {
            var logger = new DebugEventLogger();
            var settings = TestHelpers.LoadConfigFromResourceFile("UserParameters.policies", logger);

            var repository = new WorkItemRepositoryMock();
            System.Func<IRuntimeContext, IScriptLibrary> scriptLibraryBuilder = (x) => Substitute.For<IScriptLibrary>();

            var context = Substitute.For<IRequestContext>();
            context.GetProjectCollectionUri().Returns(
                new System.Uri("http://localhost:8080/tfs/DefaultCollection"));
            var runtime = RuntimeContext.MakeRuntimeContext(@"C:\GlobalList_UserParameterReplaceExisting_Succeeded", settings, context, logger, (c) => repository, scriptLibraryBuilder);

            var workItem = new WorkItemMock(repository, runtime);
            workItem.Id = 1;
            workItem.TypeName = "Use Case";
            workItem["Title"] = "The car shall have a maximum speed of {myParameter}(25) mph.";

            repository.SetWorkItems(new[] { workItem });

            using (var processor = new EventProcessor(runtime))
            {
                var notification = Substitute.For<INotification>();
                notification.WorkItemId.Returns(1);

                var result = processor.ProcessEvent(context, notification);

                Assert.AreEqual(0, result.ExceptionProperties.Count);
                Assert.IsTrue(workItem.InternalWasSaveCalled);
                Assert.AreEqual("The car shall have a maximum speed of {myParameter}(30) mph.", workItem.Fields["Title"].Value);
                Assert.AreEqual(EventNotificationStatus.ActionPermitted, result.NotificationStatus);
            }
        }
    }
}
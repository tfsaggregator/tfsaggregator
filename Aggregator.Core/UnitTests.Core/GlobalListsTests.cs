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
        const string DefaultGlobalLists = @"<gl:GLOBALLISTS xmlns:gl='http://schemas.microsoft.com/VisualStudio/2005/workitemtracking/globallists'>
  <GLOBALLIST name='Builds - Share_Migration_toolkit_for_Sharepoint'>
    <LISTITEM value='ShareMigrate2/ShareMigrate2_20130318.1' />
  </GLOBALLIST>
  <GLOBALLIST name='Builds - MyBuildTests'>
    <LISTITEM value='DumpEnvironment/DumpEnvironment_20150528.1' />
    <LISTITEM value='DumpEnvironment/DumpEnvironment_20150528.2' />
    <LISTITEM value='DumpEnvironment/DumpEnvironment_20150528.3' />
    <LISTITEM value='DumpEnvironment/DumpEnvironment_20150528.4' />
    <LISTITEM value='DumpEnvironment/DumpEnvironment_20150528.5' />
  </GLOBALLIST>
  <GLOBALLIST name='Aggregator - UserParameters'>
    <LISTITEM value= 'myParameter=30' />
  </GLOBALLIST>
</gl:GLOBALLISTS>";

        [TestMethod]
        public void GlobalList_get_Succeeded()
        {
            var repository = new WorkItemRepositoryMock(DefaultGlobalLists);

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

            var repository = new WorkItemRepositoryMock(DefaultGlobalLists);
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

            var repository = new WorkItemRepositoryMock(DefaultGlobalLists);
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

        [TestMethod]
        public void GlobalList_Add_Succeeded()
        {
            const string globalListName = "Aggregator - UserParameters";
            const string testItem = "new item";
            var repository = new WorkItemRepositoryMock(DefaultGlobalLists);

            repository.AddItemToGlobalList(globalListName, testItem);

            var gl = repository.GetGlobalList(globalListName).ToArray();
            Assert.IsNotNull(gl);
            Assert.AreEqual(2, gl.Length);
            Assert.AreEqual(testItem, gl[1]);
        }

        [TestMethod]
        public void GlobalList_AddNonExistingList_Succeeded()
        {
            const string globalListName = "Aggregator - DoesNotExists";
            const string testItem = "A new item";
            var repository = new WorkItemRepositoryMock(DefaultGlobalLists);

            repository.AddItemToGlobalList(globalListName, testItem);

            var gl = repository.GetGlobalList(globalListName).ToArray();
            Assert.IsNotNull(gl);
            Assert.AreEqual(1, gl.Length);
            Assert.AreEqual(testItem, gl[0]);
        }

        [TestMethod]
        public void GlobalList_Remove_Succeeded()
        {
            const string globalListName = "Aggregator - UserParameters";
            const string testItem = "myParameter=30";
            var repository = new WorkItemRepositoryMock(DefaultGlobalLists);

            repository.RemoveItemFromGlobalList(globalListName, testItem);

            var gl = repository.GetGlobalList(globalListName).ToArray();
            Assert.IsNotNull(gl);
            Assert.AreEqual(0, gl.Length);
        }

        [TestMethod]
        public void GlobalList_RemoveTwice_Succeeded()
        {
            const string globalListName = "Aggregator - UserParameters";
            const string testItem = "myParameter=30";
            var repository = new WorkItemRepositoryMock(DefaultGlobalLists);

            repository.RemoveItemFromGlobalList(globalListName, testItem);
            repository.RemoveItemFromGlobalList(globalListName, testItem);

            var gl = repository.GetGlobalList(globalListName).ToArray();
            Assert.IsNotNull(gl);
            Assert.AreEqual(0, gl.Length);
        }

        [TestMethod]
        public void GlobalList_RemoveNonExistingList_Succeeded()
        {
            const string globalListName = "Aggregator - DoesNotExists";
            const string testItem = "anything";
            var repository = new WorkItemRepositoryMock(DefaultGlobalLists);

            repository.RemoveItemFromGlobalList(globalListName, testItem);

            var gl = repository.GetGlobalList(globalListName).ToArray();
            Assert.IsNotNull(gl);
            Assert.AreEqual(0, gl.Length);
        }

    }
}
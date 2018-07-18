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
    public class ItemCreationTests
    {
        [TestMethod]
        public void WorkItemLink_addNew_succeeds()
        {
            var logger = new DebugEventLogger();
            var settings = TestHelpers.LoadConfigFromResourceFile("NewObjects.policies", logger);

            var repository = new WorkItemRepositoryMock();
            System.Func<IRuntimeContext, IScriptLibrary> scriptLibraryBuilder = (x) => Substitute.For<IScriptLibrary>();

            var context = Substitute.For<IRequestContext>();
            context.GetProjectCollectionUri().Returns(
                new System.Uri("http://localhost:8080/tfs/DefaultCollection"));
            var runtime = RuntimeContext.MakeRuntimeContext(@"C:\WorkItemLink_addNew_succeeds", settings, context, logger, (c) => repository, scriptLibraryBuilder);

            var parent = new WorkItemMock(repository, runtime);
            parent.Id = 1;
            parent.TypeName = "Use Case";
            parent["Title"] = "UC";

            var child = new WorkItemMock(repository, runtime);
            child.Id = 2;
            child.TypeName = "Task";
            child["Title"] = "TSK";

            repository.SetWorkItems(new[] { parent, child });

            using (var processor = new EventProcessor(runtime))
            {
                var notification = Substitute.For<INotification>();
                notification.WorkItemId.Returns(2);

                var result = processor.ProcessEvent(context, notification);

                Assert.AreEqual(0, result.ExceptionProperties.Count);
                Assert.IsTrue(child.InternalWasSaveCalled);
                Assert.AreEqual(EventNotificationStatus.ActionPermitted, result.NotificationStatus);
            }
        }

        [TestMethod]
        public void WorkItemLink_addExisting_noop()
        {
            var logger = new DebugEventLogger();
            var settings = TestHelpers.LoadConfigFromResourceFile("NewObjects.policies", logger);

            var repository = new WorkItemRepositoryMock();
            System.Func<IRuntimeContext, IScriptLibrary> scriptLibraryBuilder = (x) => Substitute.For<IScriptLibrary>();

            var context = Substitute.For<IRequestContext>();
            context.GetProjectCollectionUri().Returns(
                new System.Uri("http://localhost:8080/tfs/DefaultCollection"));
            var runtime = RuntimeContext.MakeRuntimeContext(@"C:\WorkItemLink_addExisting_noop", settings, context, logger, (c) => repository, scriptLibraryBuilder);

            var parent = new WorkItemMock(repository, runtime);
            parent.Id = 1;
            parent.TypeName = "Use Case";
            parent["Title"] = "UC";

            var child = new WorkItemMock(repository, runtime);
            child.Id = 2;
            child.TypeName = "Task";
            child["Title"] = "TSK";

            child.WorkItemLinksImpl.Add(new WorkItemLinkMock(WorkItemImplementationBase.ParentRelationship, parent.Id, repository));
            repository.SetWorkItems(new[] { parent, child });

            using (var processor = new EventProcessor(runtime))
            {
                var notification = Substitute.For<INotification>();
                notification.WorkItemId.Returns(2);

                var result = processor.ProcessEvent(context, notification);

                Assert.AreEqual(0, result.ExceptionProperties.Count);
                Assert.IsFalse(child.InternalWasSaveCalled);
                Assert.IsFalse(parent.InternalWasSaveCalled);
                Assert.AreEqual(EventNotificationStatus.ActionPermitted, result.NotificationStatus);
            }
        }

        [TestMethod]
        public void WorkItem_addNew_succeeds()
        {
            var logger = new DebugEventLogger();
            var settings = TestHelpers.LoadConfigFromResourceFile("NewObjects.policies", logger);

            var repository = new WorkItemRepositoryMock();
            System.Func<IRuntimeContext, IScriptLibrary> scriptLibraryBuilder = (x) => Substitute.For<IScriptLibrary>();

            var context = Substitute.For<IRequestContext>();
            context.GetProjectCollectionUri().Returns(
                new System.Uri("http://localhost:8080/tfs/DefaultCollection"));
            var runtime = RuntimeContext.MakeRuntimeContext(@"C:\WorkItem_addNew_succeeds", settings, context, logger, (c) => repository, scriptLibraryBuilder);

            var parent = new WorkItemMock(repository, runtime);
            parent.Id = 1;
            parent.TypeName = "Bug";
            parent[CoreFieldReferenceNames.Title] = "My bug #1";
            parent[CoreFieldReferenceNames.TeamProject] = "MyTeamProject";

            repository.SetWorkItems(new[] { parent });

            using (var processor = new EventProcessor(runtime))
            {
                var notification = Substitute.For<INotification>();
                notification.WorkItemId.Returns(1);

                var result = processor.ProcessEvent(context, notification);

                Assert.AreEqual(0, result.ExceptionProperties.Count);
                Assert.AreEqual(1, repository.LoadedWorkItems.Count);
                Assert.AreEqual(1, repository.CreatedWorkItems.Count);
            }
        }

        [TestMethod]
        public void WorkItem_addChangeset_To_Parent()
        {
            var logger = new DebugEventLogger();
            var settings = TestHelpers.LoadConfigFromResourceFile("NewObjects.policies", logger);

            var repository = new WorkItemRepositoryMock();
            System.Func<IRuntimeContext, IScriptLibrary> scriptLibraryBuilder = (x) => Substitute.For<IScriptLibrary>();

            var context = Substitute.For<IRequestContext>();
            context.GetProjectCollectionUri().Returns(
                new System.Uri("http://localhost:8080/tfs/DefaultCollection"));
            var runtime = RuntimeContext.MakeRuntimeContext(@"C:\WorkItem_addChangeset_To_Parent", settings, context, logger, (c) => repository, scriptLibraryBuilder);

            var parent = new WorkItemMock(repository, runtime);
            parent.Id = 1;
            parent.TypeName = "Bug";
            parent[CoreFieldReferenceNames.Title] = "My bug #1";
            parent[CoreFieldReferenceNames.TeamProject] = "MyTeamProject";

            var child = new WorkItemMock(repository, runtime);
            child.Id = 3;
            child.TypeName = "Task";
            child["Title"] = "Resolve bug";
            var addedChangesSet1 = new AddedResourceLink() {Resource = @"vstfs:///VersionControl/Changeset/1" };
            var addedChangesSet2 = new AddedResourceLink() {Resource = @"vstfs:///VersionControl/Changeset/2" };
            var addedSourceFile = new AddedResourceLink() { Resource = @"vstfs:///VersionControl/VersionedItem/$TFS_VC/Anypath/anyfile.cs" };
            var otherLink = new AddedResourceLink() { Resource = @"unknown" };
            child.AddedResourceLinkList.Add(addedChangesSet1);
            child.AddedResourceLinkList.Add(addedChangesSet2);
            child.AddedResourceLinkList.Add(addedSourceFile);
            child.AddedResourceLinkList.Add(otherLink);

            Assert.AreEqual(1, child.AddedResourceLinks.Count(links => links.AddedResourceLinkType == AddedResourceLinkTypes.None));
            Assert.AreEqual(1, child.AddedResourceLinks.Count(links => links.AddedResourceLinkType == AddedResourceLinkTypes.SourceFile));
            Assert.AreEqual(2, child.AddedResourceLinks.Count(links => links.AddedResourceLinkType == AddedResourceLinkTypes.Changeset));            
            child.WorkItemLinksImpl.Add(new WorkItemLinkMock(WorkItemImplementationBase.ParentRelationship, parent.Id, repository));
            repository.SetWorkItems(new[] { parent, child });

            using (var processor = new EventProcessor(runtime))
            {
                var notification = Substitute.For<INotification>();
                notification.WorkItemId.Returns(3);
                var result = processor.ProcessEvent(context, notification);

                Assert.AreEqual(0, result.ExceptionProperties.Count);
                Assert.AreEqual(2, parent.InternalAddChangesetLinkCalled);
                Assert.AreEqual(0, parent.InternalAddSourceFileLinkCalled);
                Assert.IsFalse(child.InternalWasSaveCalled);
                Assert.IsFalse(parent.InternalWasSaveCalled);
                Assert.AreEqual(EventNotificationStatus.ActionPermitted, result.NotificationStatus);
            }
        }
    }
}
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

using Aggregator.Core;
using Aggregator.Core.Context;
using Aggregator.Core.Interfaces;
using Aggregator.Core.Monitoring;

using Microsoft.TeamFoundation.Framework.Server;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using NSubstitute;

using UnitTests.Core.Mock;

namespace UnitTests.Core
{
    [TestClass]
    public class SingleParentChildAggregationsTests
    {
        private IWorkItemRepository repository;
        private IWorkItem workItem;

        private IWorkItemRepository SetupFakeRepository()
        {
            this.repository = Substitute.For<IWorkItemRepository>();

            this.workItem = Substitute.For<IWorkItem>();
            this.workItem.Id.Returns(1);
            this.workItem.TypeName.Returns("Task");
            this.workItem.Fields["Estimated Work"].Value = 0.0D;
            this.workItem.Fields["Estimated Dev Work"].Value.Returns(1.0D);
            this.workItem.Fields["Estimated Test Work"].Value.Returns(2.0D);
            this.workItem.IsValid().Returns(true);

            // triggers save
            this.workItem.IsDirty.Returns(true);

            this.repository.GetWorkItem(1).Returns(this.workItem);
            this.repository.LoadedWorkItems.Returns(new ReadOnlyCollection<IWorkItem>(new List<IWorkItem>() { this.workItem }));
            this.repository.CreatedWorkItems.Returns(new ReadOnlyCollection<IWorkItem>(new List<IWorkItem>()));

            return this.repository;
        }

        private IWorkItemRepository SetupFakeRepository_Short()
        {
            this.repository = Substitute.For<IWorkItemRepository>();

            this.workItem = Substitute.For<IWorkItem>();
            this.workItem.Id.Returns(1);
            this.workItem.TypeName.Returns("Task");
            this.workItem["Estimated Dev Work"].Returns(1.0D);
            this.workItem["Estimated Test Work"].Returns(2.0D);
            this.workItem["Finish Date"].Returns(new DateTime(2010, 1, 1));
            this.workItem.IsValid().Returns(true);

            // triggers save
            this.workItem.IsDirty.Returns(true);

            this.repository.GetWorkItem(1).Returns(this.workItem);
            this.repository.LoadedWorkItems.Returns(new ReadOnlyCollection<IWorkItem>(new List<IWorkItem>() { this.workItem }));
            this.repository.CreatedWorkItems.Returns(new ReadOnlyCollection<IWorkItem>(new List<IWorkItem>()));
            return this.repository;
        }

        [TestMethod]
        public void Should_aggregate_a_numeric_field()
        {
            var logger = Substitute.For<ILogEvents>();
            var settings = TestHelpers.LoadConfigFromResourceFile("SumFieldsOnSingleWorkItem.policies", logger);
            var alternateRepository = this.SetupFakeRepository();
            System.Func<IRuntimeContext, IScriptLibrary> scriptLibraryBuilder = (x) => Substitute.For<IScriptLibrary>();
            var context = Substitute.For<IRequestContext>();
            context.GetProjectCollectionUri().Returns(
                new System.Uri("http://localhost:8080/tfs/DefaultCollection"));
            var runtime = RuntimeContext.MakeRuntimeContext(@"C:\Should_aggregate_a_numeric_field", settings, context, logger, (c) => alternateRepository, scriptLibraryBuilder);
            using (var processor = new EventProcessor(runtime))
            {
                var notification = Substitute.For<INotification>();
                notification.WorkItemId.Returns(1);

                var result = processor.ProcessEvent(context, notification);

                Assert.AreEqual(0, result.ExceptionProperties.Count);
                this.workItem.Received().Save();
                Assert.AreEqual(3.0D, this.workItem.Fields["Estimated Work"].Value);
                Assert.AreEqual(EventNotificationStatus.ActionPermitted, result.NotificationStatus);
            }
        }

        [TestMethod]
        public void Should_aggregate_a_numeric_field_short()
        {
            var logger = Substitute.For<ILogEvents>();
            var settings = TestHelpers.LoadConfigFromResourceFile("SumFieldsOnSingleWorkItem-Short.policies", logger);
            var alternateRepository = this.SetupFakeRepository_Short();
            System.Func<IRuntimeContext, IScriptLibrary> scriptLibraryBuilder = (x) => Substitute.For<IScriptLibrary>();
            var context = Substitute.For<IRequestContext>();
            context.GetProjectCollectionUri().Returns(
                new System.Uri("http://localhost:8080/tfs/DefaultCollection"));
            var runtime = RuntimeContext.MakeRuntimeContext(@"C:\Should_aggregate_a_numeric_field_short", settings, context, logger, (c) => alternateRepository, scriptLibraryBuilder);
            using (var processor = new EventProcessor(runtime))
            {
                var notification = Substitute.For<INotification>();
                notification.WorkItemId.Returns(1);

                var result = processor.ProcessEvent(context, notification);

                Assert.AreEqual(0, result.ExceptionProperties.Count);
                this.workItem.Received().Save();
                Assert.AreEqual(3.0D, this.workItem["Estimated Work"]);
                Assert.AreEqual(EventNotificationStatus.ActionPermitted, result.NotificationStatus);
            }
        }

        [TestMethod]
        public void Should_aggregate_a_numeric_field_VB()
        {
            var logger = Substitute.For<ILogEvents>();
            var settings = TestHelpers.LoadConfigFromResourceFile("SumFieldsOnSingleWorkItemVB.policies", logger);
            var alternateRepository = this.SetupFakeRepository_Short();
            System.Func<IRuntimeContext, IScriptLibrary> scriptLibraryBuilder = (x) => Substitute.For<IScriptLibrary>();
            var context = Substitute.For<IRequestContext>();
            context.GetProjectCollectionUri().Returns(
                new System.Uri("http://localhost:8080/tfs/DefaultCollection"));
            var runtime = RuntimeContext.MakeRuntimeContext(@"C:\Should_aggregate_a_numeric_field_VB", settings, context, logger, (c) => alternateRepository, scriptLibraryBuilder);
            using (var processor = new EventProcessor(runtime))
            {
                var notification = Substitute.For<INotification>();
                notification.WorkItemId.Returns(1);

                var result = processor.ProcessEvent(context, notification);

                Assert.AreEqual(0, result.ExceptionProperties.Count);
                this.workItem.Received().Save();
                Assert.AreEqual(3.0D, this.workItem["Estimated Work"]);
                Assert.AreEqual(EventNotificationStatus.ActionPermitted, result.NotificationStatus);
            }
        }

        [TestMethod]
        public void Should_aggregate_to_parent()
        {
            var logger = Substitute.For<ILogEvents>();
            var settings = TestHelpers.LoadConfigFromResourceFile("Rollup.policies", logger);
            var alternateRepository = new WorkItemRepositoryMock();
            System.Func<IRuntimeContext, IScriptLibrary> scriptLibraryBuilder = (x) => Substitute.For<IScriptLibrary>();

            var context = Substitute.For<IRequestContext>();
            context.GetProjectCollectionUri().Returns(
                new System.Uri("http://localhost:8080/tfs/DefaultCollection"));
            context.CollectionName.Returns("Collection1");
            var runtime = RuntimeContext.MakeRuntimeContext(@"C:\Should_aggregate_to_parent", settings, context, logger, (c) => alternateRepository, scriptLibraryBuilder);

            var grandParent = new WorkItemMock(alternateRepository, runtime);
            grandParent.Id = 1;
            grandParent.TypeName = "Feature";
            grandParent["Dev Estimate"] = 0.0D;
            grandParent["Test Estimate"] = 0.0D;

            var parent = new WorkItemMock(alternateRepository, runtime);
            parent.Id = 2;
            parent.TypeName = "Use Case";
            parent.WorkItemLinksImpl.Add(new WorkItemLinkMock("Parent", 1, alternateRepository));
            grandParent.WorkItemLinksImpl.Add(new WorkItemLinkMock("Child", 2, alternateRepository));
            parent["Total Work Remaining"] = 3.0D;
            parent["Total Estimate"] = 4.0D;

            var child = new WorkItemMock(alternateRepository, runtime);
            child.Id = 3;
            child.TypeName = "Task";
            child.WorkItemLinksImpl.Add(new WorkItemLinkMock("Parent", 2, alternateRepository));
            parent.WorkItemLinksImpl.Add(new WorkItemLinkMock("Child", 3, alternateRepository));
            child["Estimated Dev Work"] = 10.0D;
            child["Estimated Test Work"] = 20.0D;
            child["Remaining Dev Work"] = 1.0D;
            child["Remaining Test Work"] = 2.0D;
            child["Finish Date"] = new DateTime(2015, 1, 1);

            child.WorkItemLinksImpl.Add(new WorkItemLinkMock(WorkItemImplementationBase.ParentRelationship, parent.Id, alternateRepository));
            parent.WorkItemLinksImpl.Add(new WorkItemLinkMock(WorkItemImplementationBase.ParentRelationship, grandParent.Id, alternateRepository));
            alternateRepository.SetWorkItems(new[] { grandParent, parent, child });

            using (var processor = new EventProcessor(runtime))
            {
                var notification = Substitute.For<INotification>();
                notification.WorkItemId.Returns(3);

                var result = processor.ProcessEvent(context, notification);

                Assert.AreEqual(0, result.ExceptionProperties.Count);
                Assert.IsFalse(child.InternalWasSaveCalled);
                Assert.IsTrue(parent.InternalWasSaveCalled);
                Assert.IsFalse(grandParent.InternalWasSaveCalled);
                Assert.AreEqual(3.0D, parent["Total Work Remaining"]);
                Assert.AreEqual(30.0D, parent["Total Estimate"]);
                Assert.AreEqual(EventNotificationStatus.ActionPermitted, result.NotificationStatus);
            }
        }

        [TestMethod]
        public void Should_aggregate_to_parent_should_handle_null()
        {
            var logger = Substitute.For<ILogEvents>();
            var settings = TestHelpers.LoadConfigFromResourceFile("Rollup.policies", logger);
            var alternateRepository = new WorkItemRepositoryMock();
            System.Func<IRuntimeContext, IScriptLibrary> scriptLibraryBuilder = (x) => Substitute.For<IScriptLibrary>();

            var context = Substitute.For<IRequestContext>();
            context.GetProjectCollectionUri().Returns(
                new System.Uri("http://localhost:8080/tfs/DefaultCollection"));
            var runtime = RuntimeContext.MakeRuntimeContext(@"C:\Should_aggregate_to_parent_should_handle_null", settings, context, logger, (c) => alternateRepository, scriptLibraryBuilder);

            var grandParent = new WorkItemMock(alternateRepository, runtime);
            grandParent.Id = 1;
            grandParent.TypeName = "Feature";
            grandParent["Dev Estimate"] = null;
            grandParent["Test Estimate"] = null;

            var parent = new WorkItemMock(alternateRepository, runtime);
            parent.Id = 2;
            parent.TypeName = "Use Case";
            parent.WorkItemLinksImpl.Add(new WorkItemLinkMock("Parent", 1, alternateRepository));
            grandParent.WorkItemLinksImpl.Add(new WorkItemLinkMock("Child", 2, alternateRepository));
            parent["Total Work Remaining"] = 3.0D;
            parent["Total Estimate"] = 4.0D;

            var child = new WorkItemMock(alternateRepository, runtime);
            child.Id = 3;
            child.TypeName = "Task";
            child.WorkItemLinksImpl.Add(new WorkItemLinkMock("Parent", 2, alternateRepository));
            parent.WorkItemLinksImpl.Add(new WorkItemLinkMock("Child", 3, alternateRepository));
            child["Estimated Dev Work"] = 10.0D;
            child["Estimated Test Work"] = 20.0D;
            child["Remaining Dev Work"] = null;
            child["Remaining Test Work"] = 2.0D;
            child["Finish Date"] = new DateTime(2015, 1, 1);

            child.WorkItemLinksImpl.Add(new WorkItemLinkMock(WorkItemImplementationBase.ParentRelationship, parent.Id, alternateRepository));
            parent.WorkItemLinksImpl.Add(new WorkItemLinkMock(WorkItemImplementationBase.ParentRelationship, grandParent.Id, alternateRepository));
            alternateRepository.SetWorkItems(new[] { grandParent, parent, child });

            using (var processor = new EventProcessor(runtime))
            {
                var notification = Substitute.For<INotification>();
                notification.WorkItemId.Returns(3);

                var result = processor.ProcessEvent(context, notification);

                Assert.AreEqual(0, result.ExceptionProperties.Count);
                Assert.IsFalse(child.InternalWasSaveCalled);
                Assert.IsTrue(parent.InternalWasSaveCalled);
                Assert.IsFalse(grandParent.InternalWasSaveCalled);
                Assert.AreEqual(2.0D, parent["Total Work Remaining"]);
                Assert.AreEqual(30.0D, parent["Total Estimate"]);
                Assert.AreEqual(EventNotificationStatus.ActionPermitted, result.NotificationStatus);
            }
        }
    }
}

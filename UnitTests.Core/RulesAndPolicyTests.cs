using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Aggregator.Core.Context;
using Aggregator.Core.Interfaces;
using Aggregator.Core.Monitoring;

namespace UnitTests.Core
{
    using Aggregator.Core;
    using Aggregator.Core.Configuration;
    using Aggregator.Core.Navigation;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using NSubstitute;
    using System;
    using UnitTests.Core.Mock;

    [TestClass]
    public class RulesAndPolicyTests
    {
        private WorkItemRepositoryMock MakeRepositoryMock()
        {
            var repository = new WorkItemRepositoryMock();
            var parent = new WorkItemMock(repository);
            parent.Id = 1;
            parent.TypeName = "Use Case";
            parent["Title"] = "UC";

            var child = new WorkItemMock(repository);
            child.Id = 2;
            child.TypeName = "Task";
            child["Title"] = "TSK";

            child.WorkItemLinks.Add(new WorkItemLinkMock(WorkItemImplementationBase.ParentRelationship, parent.Id, repository));
            repository.SetWorkItems(new[] { parent, child });

            return repository;
        }

        [TestMethod]
        public void RulesAndPolicy_SpecificCollection_TwoPoliciesTwoRulesApplies()
        {
            var logger = Substitute.For<ILogEvents>();
            var settings = TestHelpers.LoadConfigFromResourceFile("RulesAndPolicy.policies", logger);
            var repository = MakeRepositoryMock();
            var context = Substitute.For<IRequestContext>();
            context.CollectionName.Returns("Collection1");
            var runtime = RuntimeContext.MakeRuntimeContext("settingsPath", settings, context, logger);
            var processor = new EventProcessor(repository, runtime);
            var notification = Substitute.For<INotification>();
            notification.WorkItemId.Returns(2);

            var result = processor.ProcessEvent(context, notification);

            Assert.AreEqual(0, result.ExceptionProperties.Count());
            Assert.AreEqual(Microsoft.TeamFoundation.Framework.Server.EventNotificationStatus.ActionPermitted, result.NotificationStatus);
            object expected = 42;
            logger.Received().ResultsFromScriptRun("Noop1", expected);
            logger.Received().ResultsFromScriptRun("Noop2", expected);
            logger.DidNotReceive().ResultsFromScriptRun("Noop3", expected);
        }

        [TestMethod]
        public void RulesAndPolicy_GenericCollection_OnePoliciesOneRulesApplies()
        {
            var logger = Substitute.For<ILogEvents>();
            var settings = TestHelpers.LoadConfigFromResourceFile("RulesAndPolicy.policies", logger);
            var repository = MakeRepositoryMock();
            var context = Substitute.For<IRequestContext>();
            context.CollectionName.Returns("Collection2");
            var runtime = RuntimeContext.MakeRuntimeContext("settingsPath", settings, context, logger);
            var processor = new EventProcessor(repository, runtime);
            var notification = Substitute.For<INotification>();
            notification.WorkItemId.Returns(2);

            var result = processor.ProcessEvent(context, notification);

            Assert.AreEqual(0, result.ExceptionProperties.Count());
            Assert.AreEqual(Microsoft.TeamFoundation.Framework.Server.EventNotificationStatus.ActionPermitted, result.NotificationStatus);
            object expected = 42;
            logger.DidNotReceive().ResultsFromScriptRun("Noop1", expected);
            logger.Received().ResultsFromScriptRun("Noop2", expected);
            logger.DidNotReceive().ResultsFromScriptRun("Noop3", expected);
        }

        [TestMethod]
        public void RulesAndPolicy_TypeFilter_OnePoliciesOneRulesApplies()
        {
            var logger = Substitute.For<ILogEvents>();
            var settings = TestHelpers.LoadConfigFromResourceFile("RulesAndPolicy.policies", logger);
            var repository = new WorkItemRepositoryMock();
            var workItem = new WorkItemMock(repository);
            workItem.Id = 1;
            workItem.TypeName = "Bug";
            workItem["Title"] = "My bug";
            repository.SetWorkItems(new[] { workItem });
            var context = Substitute.For<IRequestContext>();
            context.CollectionName.Returns("Collection2");
            var runtime = RuntimeContext.MakeRuntimeContext("settingsPath", settings, context, logger);
            var processor = new EventProcessor(repository, runtime);
            var notification = Substitute.For<INotification>();
            notification.WorkItemId.Returns(1);

            var result = processor.ProcessEvent(context, notification);

            Assert.AreEqual(0, result.ExceptionProperties.Count());
            Assert.AreEqual(Microsoft.TeamFoundation.Framework.Server.EventNotificationStatus.ActionPermitted, result.NotificationStatus);
            object expected = 42;
            logger.DidNotReceive().ResultsFromScriptRun("Noop1", expected);
            logger.DidNotReceive().ResultsFromScriptRun("Noop2", expected);
            logger.Received().ResultsFromScriptRun("Noop3", expected);
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitTests.Core
{
    using Aggregator.Core;
    using Aggregator.Core.Navigation;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using NSubstitute;
    using UnitTests.Core.Mock;

    [TestClass]
    public class Navigation
    {
        private static IWorkItemRepository MakeRepository(out IWorkItem startPoint)
        {
            var repository = new WorkItemRepositoryMock();

            var grandParent = new WorkItemMock(repository);
            grandParent.Id = 1;
            grandParent.TypeName = "Feature";

            var parent = new WorkItemMock(repository);
            parent.Id = 2;
            parent.TypeName = "Use Case";

            var firstChild = new WorkItemMock(repository);
            firstChild.Id = 3;
            firstChild.TypeName = "Task";
            var secondChild = new WorkItemMock(repository);
            secondChild.Id = 4;
            secondChild.TypeName = "Task";

            firstChild.WorkItemLinks.Add(new WorkItemLinkMock(WorkItemLazyReference.ParentRelationship, parent.Id, repository));
            secondChild.WorkItemLinks.Add(new WorkItemLinkMock(WorkItemLazyReference.ParentRelationship, parent.Id, repository));
            parent.WorkItemLinks.Add(new WorkItemLinkMock(WorkItemLazyReference.ParentRelationship, grandParent.Id, repository));

            grandParent.WorkItemLinks.Add(new WorkItemLinkMock(WorkItemLazyReference.ChildRelationship, parent.Id, repository));
            parent.WorkItemLinks.Add(new WorkItemLinkMock(WorkItemLazyReference.ChildRelationship, firstChild.Id, repository));
            parent.WorkItemLinks.Add(new WorkItemLinkMock(WorkItemLazyReference.ChildRelationship, secondChild.Id, repository));

            repository.SetWorkItems(new[] { grandParent, parent, firstChild, secondChild });

            startPoint = grandParent;
            return repository;
        }

        [TestMethod]
        public void FluentNavigation_succeedes()
        {
            IWorkItem startPoint;
            var repository = MakeRepository(out startPoint);
            var logger = Substitute.For<ILogEvents>();

            var searchResult = startPoint
                .WhereTypeIs("Task")
                .AtMost(2)
                .FollowingLinks("*")
                .ToArray();

            Assert.AreEqual(2, searchResult.Length);
            Assert.AreEqual(3, searchResult[0].Id);
            Assert.AreEqual(4, searchResult[1].Id);
        }

        [TestMethod]
        public void FluentNavigation_CSharp_script_succeedes()
        {
            string script = @"
var searchResult = self
    .WhereTypeIs(""Task"")
    .AtMost(2)
    .FollowingLinks(""*"");
return searchResult;
";
            IWorkItem startPoint;
            var repository = MakeRepository(out startPoint);
            var logger = Substitute.For<ILogEvents>();

            var engine = new CSharpScriptEngine(repository, logger);
            engine.LoadAndRun("test", script, startPoint);

            var expected = new FluentQuery(startPoint);
            expected
                .WhereTypeIs("Task")
                .AtMost(2)
                .FollowingLinks("*");
            logger.Received().ResultsFromScriptRun("test", (object)expected);
        }
    }
}

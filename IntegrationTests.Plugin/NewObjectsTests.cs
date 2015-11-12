using IntegrationTests.Plugin;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using System;
using System.Linq;
using System.Threading;
using Xunit;

// avoid namespaces with xUnit

[Collection("TFS collection")]
public class NewObjectsTests
{
    TfsFixture fixture;

    public NewObjectsTests(TfsFixture fixture)
    {
        this.fixture = fixture;
    }

    [Fact]
    public void NewLink_succeeds()
    {
        // prepare
        this.fixture.PushPolicies("NewObjects");

        string workItemTypeName = "Product Backlog Item";
        var targetType = this.fixture.Project.WorkItemTypes[workItemTypeName];
        var wiPBI = new WorkItem(targetType);
        wiPBI.Title = workItemTypeName + " NewLink_succeeds";
        wiPBI.Save();

        workItemTypeName = "Task";
        targetType = this.fixture.Project.WorkItemTypes[workItemTypeName];
        var wiTask = new WorkItem(targetType);
        wiTask.Title = workItemTypeName + " NewLink_succeeds";

        // trigger
        wiTask.Save();
        Thread.Sleep(3000);
        wiPBI.SyncToLatest();
        wiTask.SyncToLatest();

        // validate
        var hierarchyRelationship = this.fixture.WorkItemStore.WorkItemLinkTypes["System.LinkTypes.Hierarchy"];
        Assert.Equal(1, wiTask.WorkItemLinks.Count);
        var actualLink = wiTask.WorkItemLinks[0];
        Assert.Equal(hierarchyRelationship.ReverseEnd.ImmutableName, actualLink.LinkTypeEnd.ImmutableName);
        Assert.Equal(wiTask.Id, actualLink.SourceId);
        Assert.Equal(1, actualLink.TargetId);
    }

    [Fact]
    public void NewTask_succeeds()
    {
        // prepare
        this.fixture.PushPolicies("NewObjects");

        string workItemTypeName = "Bug";
        var targetType = this.fixture.Project.WorkItemTypes[workItemTypeName];
        var wiBug = new WorkItem(targetType);
        wiBug.Title = workItemTypeName + " NewLink_succeeds";

        // trigger
        wiBug.Save();
        Thread.Sleep(3000);
        wiBug.SyncToLatest();

        // validate
        var hierarchyRelationship = this.fixture.WorkItemStore.WorkItemLinkTypes["System.LinkTypes.Hierarchy"];
        Assert.Equal(1, wiBug.WorkItemLinks.Count);
        var actualLink = wiBug.WorkItemLinks[0];
        Assert.Equal(hierarchyRelationship.ForwardEnd.ImmutableName, actualLink.LinkTypeEnd.ImmutableName);
        Assert.Equal(wiBug.Id, actualLink.SourceId);
        var wiTask = fixture.WorkItemStore.GetWorkItem(actualLink.TargetId);
        Assert.NotNull(wiTask);
        Assert.Equal("Task", wiTask.Type.Name);
        Assert.Equal("Task auto-generated for " + wiBug.Title, wiTask.Title);
    }
}

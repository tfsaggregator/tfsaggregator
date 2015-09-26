# Languages

## `C#`
You can use only types from `System.dll` or `Microsoft.TeamFoundation.WorkItemTracking.Client`.
Any other reference will result in compile errors. 



# Object Model

Aggregator exposes two pre-defined variables:

 - `self` as the pivot for all computation.
 - `store` to access the entire set of work items.

## self

Represents the work item that triggered the rule.
You can access the [Fields](https://msdn.microsoft.com/en-us/library/microsoft.teamfoundation.workitemtracking.client.field.aspx) using either syntax:
```
self.Field["field_name"]
```
or simply
```
self["field_name"]
```

Prefer using Reference names e.g. `System.Title`.

For numeric and dates you may prefer using
```
var num = self.GetField<int>("field_name", 42);
```

The [`IsValid`](https://msdn.microsoft.com/en-us/library/microsoft.teamfoundation.workitemtracking.client.workitem.isvalid.aspx) method is important to check is you set some invalid field value on a work item.

You can get the [`Id`](https://msdn.microsoft.com/en-us/library/microsoft.teamfoundation.workitemtracking.client.workitem.id.aspx) and [`TypeName`](https://msdn.microsoft.com/en-us/library/microsoft.teamfoundation.workitemtracking.client.workitemtype.name.aspx) of a work item and check the [`History`](https://msdn.microsoft.com/en-us/library/microsoft.teamfoundation.workitemtracking.client.workitem.history.aspx).


## store

Represents the current Collection's Work Items and exposes only two methods `GetWorkItem` and `MakeNewWorkItem`.

### GetWorkItem

Retrieves a work item from the current Collection by ID.

```
var myWorkitem = store.GetWorkItem(42);
```

### MakeNewWorkItem
Add a new WorkItem to current Collection.

```
var newWorkItem = store.MakeNewWorkItem("MyProject", "Bug");
```

You must specify the project and the type. The new work item Fields have default values;
it is not committed to the database until all the rules have fired and Aggregator returns control to TFS.


## logger

Allows to add a trace message to the log output via the `Log` method.
It works like `Console.WriteLine`, accepting a format string followed by optional arguments.
If you do not specify the importance, the message will be logged at `Verbose` level.

### Example

```
logger.Log("Hello, World from {1} #{0}!", self.Id, self.TypeName);
```


## Parent
Helper property to navigate a work item's parent in the Parent-Child hierarchy.

```
self.Parent["System.Title"]
```

## Children
Helper property to navigate a work item's children in the Parent-Child hierarchy.

```
foreach (var child in self.Children)
{
   if (child.TypeName == "Bug")
   {
      //...
   }
}
```

## HasParent / HasChildren / HasRelation
Helper methods to avoid referencing null properties.
```
if (self.HasParent()) {
   self.Parent["System.Title"] = "*** " + self.Parent["System.Title"];
}
```
Use the Immutable name of the Link Type, e.g. `System.LinkTypes.Hierarchy-Reverse` instead of `Parent`.

## Fluent Queries

You can get work items related using the utility methods to build a query.

 - `WhereTypeIs` filters on work item type
 - `AtMost` depth of search, i.e. maximum number of links to follow
 - `FollowingLinks` filters on link type

It is particularly useful for traversing many links.

### Example

```
var tests = self.FollowingLinks("Tested-By").WhereTypeIs("Test Case");
foreach (var test in tests)
{
   if (test["Microsoft.VSTS.Common.Severity"] == "1 - Critical") {
      // do something
   }
}
```

## Linq

You can use Linq queries on these collections:
 - `Children`
 - `Fields`

### Example

Roll-up code
```
var totalEffort = self.Parent.Children.Where(child => child.TypeName == "Task").Sum(child => child.GetField("TaskEffort", 0));
```

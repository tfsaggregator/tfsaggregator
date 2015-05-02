# Languages

## C\#
You can use only types from `System.dll` or `Microsoft.TeamFoundation.WorkItemTracking.Client`. Any other reference will result in compile errors. 

# Object Model

Aggregator exposes a pre-defined variable `self` as the pivot for all computation.

## self

Represents the work item that triggered the rule. You can access the [Fields](https://msdn.microsoft.com/en-us/library/microsoft.teamfoundation.workitemtracking.client.field.aspx) using either syntax:
```
self.Field["field_name"]
```
or simply
```
self["field_name"]
```

For numeric and dates you may prefer using
```
var num = self.GetField<int>("", 42);
```

The [`IsValid`](https://msdn.microsoft.com/en-us/library/microsoft.teamfoundation.workitemtracking.client.workitem.isvalid.aspx) method is important to check is you set some invalid field value on a work item.

You can get the [`Id`](https://msdn.microsoft.com/en-us/library/microsoft.teamfoundation.workitemtracking.client.workitem.id.aspx) and [`TypeName`](https://msdn.microsoft.com/en-us/library/microsoft.teamfoundation.workitemtracking.client.workitemtype.name.aspx) of a work item and check the [`History`](https://msdn.microsoft.com/en-us/library/microsoft.teamfoundation.workitemtracking.client.workitem.history.aspx).

## Parent
Helper property to navigate a work item's parent in the Parent-Child hierarchy.

```
self.Parent["Title"]
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

## Fluent Queries

You can get work items related using the utility methods to build a query.

 - `WhereTypeIs` filters on work item type
 - `AtMost` depth of search, i.e. maximum number of links to follow
 - `FollowingLinks` filters on link type

### Example

```
var tests = self.WhereTypeIs("Test Case").FollowingLinks("Tested-By");
foreach (var test in tests)
{
   test[""];
}
```
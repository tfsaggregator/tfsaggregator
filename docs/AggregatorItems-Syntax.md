AggregatorItems.xml Options
================================================

```
<?xml version="1.0" encoding="utf-8"?>
```

This is the basic beginning to an xml file. Do not change it.

```
<AggregatorItems tfsServerUrl="http://vsalm:8080/tfs/FabrikamFiberCollection" domain="." username="Administrator" password="P2ssw0rd" loggingVerbosity="Diagnostic" >
```

**AggregatorItems**: The main node for all the configuration options. (Single)

 - **tfsServerURL**: This is the URL for your TFS Server. (Required)
 - **domain**: This is the domain for TFS Aggregator to use with your TFS Server. (Optional)
 - **username**: This is the username for TFS Aggregator to use with your TFS Server. (Optional)
 - **password**: This is the password for TFS Aggregator to use with your TFS Server. (Optional)
 - **loggingVerbosity**: The level of logging that will be sent to *DebugView*.
Valid values are `Normal` and `Diagnostic`.
Normal will show only exception messages, Diagnostic will show complete logging.
The default value is Normal. (Optional) - Requires TFS 2012 version or higher.
See the Help page for more info: [TFS Aggregator Troubleshooting](Troubleshooting.md)

```
    <AggregatorItem name="SumTaskEstimatesToParent"
       operationType="Numeric" operation="Sum"
       linkType="Parent" linkLevel="2" siblingType="Task"
       workItemType="Task">
```

**AggregatorItem**: Represents a single aggregation rule. (Repeatable)

 - **name**: The name of this aggregation. Used to differentiate aggregations in the Debug Log.
   (Optional) - Requires TFS 2012 version or higher. See the Help page for more info: [TFS Aggregator Troubleshooting](Troubleshooting.md)
 - **operationType**: Indicates if the aggregation will be performing "math" or aggregating mapped text values.
   Valid options are: `Numeric` or `String`. (Default is Numeric)
   If Numeric is specified, then your field type must be a numeric type (i.e. double or integer).
 - **operation**: Indicates what mathematical operation will be performed for the aggregation.
   When operationType is Numeric, the valid options are: `Sum`, `Subtract`, `Multiply` or `Divide`. (Default is Sum)
   When operationType is String, the valid options are: `Sum` or `Copy`. (Default is Sum)
 - **linkType**: Indicates if the target for this aggregation is the same as the source or if it is a parent of the source.
   Valid options are: `Self` or `Parent` (Default is Self)
 - **linkLevel**: If linkType is Parent then linkLevel indicates how many "parents" up to go.
   (So linkLevel="2" will find the "grandparent" of the source for the aggregation.) (Default is 1)
 - **siblingType**: If linkType is Parent then siblingType indicates the allowed other child work item types are included in the operation.
   (Default same as `workItemType`)
 - **workItemType**: The name of the work item type that this aggregation will target. (Required)

```
        <TargetItem name="Total Estimate"/>
```

**TargetItem**: Represents the TFS Field to be updated in the aggregation. (Single)

 - **name**: The TFS field name[1] of the location to write the aggregation.

```
        <SourceItem name="Estimated Dev Work"/>
```

**SourceItem**: Represents a TFS Field to pull data from for an aggregation. (Repeatable)

 - **name**: The TFS field name[1] of the location to pull data from for the aggregation.

```
        <Conditions>
```

**Conditions**: Represents a list of conditions, of which if any are false will block the aggregation from happening.
If all are true then the aggregation will run (Single, Optional)

```
            <Condition leftField="Finish Date" operator="GreaterThan" rightValue="$NOW$"/>
```

**Condition**: Represents a single condition in a list of conditions. Note: if this is incorrectly entered then it will default to true (meaning the condition will pass). (Repeatable) 

 - **leftField**: The TFS field name[1] to use as the left value of a condition. This taken from the *Target Work Item* (Required)
 - **operator**: The comparison operator to use for this condition.
If the type of the TFS Field (leftField) is String then EqualTo will be used regardless of what is entered.
Valid options are: `LessThan`, `GreaterThan`, `LessThanOrEqualTo`, `GreaterThanOrEqualTo`. `EqualTo`, `NotEqualTo` (Default is EqualTo)
 - **rightValue**: Value to compare the leftField to.
If this is set to `$NOW$` then the current date time will be entered. If it is set to `$NULL$` then null will be used.
(Either rightValue or rightField is required)
 - **rightField**: the TFS field name[1] to use as the right value.
This taken from the *Target Work Item*. Will be compared to the left value. Not used if rightValue is specified.

```
        <Mappings>
```

**Mappings**: Represents a list mappings to allow string aggregations. (Optional)

```
            <Mapping targetValue="Done" inclusive="And">
```

**Mapping**: Represents a single mapping possibility for an aggregation.
If satisfied then the mapping will result in the TargetItem for the AggregatorItem being set to the mapping's targetValue (Repeatable)

 - **targetValue**: The actual value that the TargetItem of the AggregatorItem will be set to if the mapping is satisfied. (Required)
 - **inclusive**: This decides the inclusivity or exclusivity of the mapping.
Used to decide if all (And) or just one (Or) of the source values have to be met to cause a mapping to be valid.
Valid options are: `And` or `Or` (Default is And)

```
                <SourceValue>Removed</SourceValue>
```

**SourceValue**: Represents a possible value for the mapping.
If the AggregatorItem SourceItem's value is listed as a SourceValue then the mapping will have a match on that work item. (Repeatable)



[1] **Note** this is not the refname attribute on a TFS Field. It is the name attribute.
(So for `<FIELD name="State" refname="System.State" type="String" />` you would enter `State` NOT `System.State`.)

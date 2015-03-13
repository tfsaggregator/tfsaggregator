Example Aggregations
================================================

```
<!--Add up the estimated work on the task-->
<AggregatorItem operationType="Numeric" operation="Sum" linkType="Self" workItemType="Task">
    <TargetItem name="Estimated Work"/>
    <SourceItem name="Estimated Dev Work"/>
    <SourceItem name="Estimated Test Work"/>
</AggregatorItem>
```

This aggregation will total the values found in the Estimated Dev Work and Estimated Test Work fields for any Task work item.
The total will be placed in the Estimated Work field on the same work item as the source values were found.

```
<!--Add the time from the task up to the parent (Bug or PBI)-->
<AggregatorItem operation="Sum" linkType="Parent" linkLevel="1" workItemType="Task">
  <TargetItem name="Total Estimate"/>
  <SourceItem name="Estimated Dev Work"/>
  <SourceItem name="Estimated Test Work"/>
</AggregatorItem>
```

This aggregation will total the values found in the Estimated Dev Work and Estimated Test Work fields for all Task work items on the parent.
The total will go in the Total Estimate field on the parent one level up from the Task (i.e. the direct parent).
In the Microsoft Visual Studio Scrum template that is always a Bug or Product Backlog Item. 

```
<!--Get the total of estimated work on the Sprint-->
<AggregatorItem operationType="Numeric" operation="Sum" linkType="Parent" linkLevel="2" workItemType="Task">
  <Conditions>
    <Condition leftField="Finish Date" operator="GreaterThan" rightValue="$NOW$"/>
  </Conditions>
  <TargetItem name="Total Estimate"/>
  <SourceItem name="Estimated Dev Work"/>
  <SourceItem name="Estimated Test Work"/>
</AggregatorItem>
```

In this aggregation a condition is added.
The condition is that the field Finish Date has to be greater than the current date time on the target work item (meaning in the future).
As long as that is true then the aggregation will happen.
(The idea is that, after the sprint is over, if we move tasks from this sprint to the next, we don't want to lose the estimates that are saved on the work item.)

The Target Work Item is the second level parent of a task.
In many templates this would be the Sprint (or some other iteration like) work item.
That means that we only want to do this update if the sprint is not over.
If it is not over then this aggregation will total all values found in the Estimated Dev Work and Estimated Test Work fields for all the tasks in the sprint and Total Estimate on the sprint.

```
  <!--When all Tasks are done being worked on set the parent (Bug or PBI) to Done (unless it is Removed)-->
  <AggregatorItem operationType="String" linkType="Parent" linkLevel="1" workItemType="Task">
    <Mappings>
      <Mapping targetValue="Done" inclusive="And">
        <SourceValue>Removed</SourceValue>
        <SourceValue>Done</SourceValue>
      </Mapping>
    </Mappings>
    <Conditions>
      <Condition leftField="State" operator="NotEqualTo" rightValue="Removed"/>
    </Conditions>
    <TargetItem name="State"/>
    <SourceItem name="State"/>
  </AggregatorItem>

  <!--When any Tasks are In Progress set the parent (Bug or PBI) to In Progress-->
  <AggregatorItem operationType="String" linkType="Parent" linkLevel="1" workItemType="Task">
    <Mappings>
      <Mapping targetValue="In Progress" inclusive="Or">
        <SourceValue>In Progress</SourceValue>
        <SourceValue>Ready For Test</SourceValue>
      </Mapping>
    </Mappings>
    <TargetItem name="State"/>
    <SourceItem name="State"/>
  </AggregatorItem>
```

These are two very similar aggregations. Both target the parent of a task.
They are both targeting the state of the parent.
For the first aggregation the state of all the tasks on the parent must be either Removed or Done.
Also the parent must not be in the state of Removed.
If these things all checkout then the State of the parent will be set to done. (If not nothing is done).
For the second aggregation if any of the tasks on the parent are set to In Progress or Ready For Test then the parent will have its state set to In Progress. (The inclusive attribute is what makes the big difference here).

> **Note on States**: TFS has controls setup on State Transitions.
> Most templates do not allow you to go directly from a New state to a Done state.
> TFS Aggregator will cycle the target work item through what ever states it needs to to find the **shortest route** to the target state.
> (For most templates that is also the route that makes the most sense from a business perspective too.)

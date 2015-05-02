
## Converting rules

Add up the estimated work on the task.

```
<AggregatorItem operationType="Numeric" operation="Sum" linkType="Self" workItemType="Task">
    <TargetItem name="Estimated Work"/>
    <SourceItem name="Estimated Dev Work"/>
    <SourceItem name="Estimated Test Work"/>
</AggregatorItem>
```

```
<rule name="Sum" appliesTo="Task" hasFields="Title,Description"><![CDATA[
    self["Estimated Work"] = (double)self["Estimated Dev Work"] + (double)self["Estimated Test Work"];
]]></rule>
```

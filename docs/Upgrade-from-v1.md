
## Upgrade binaries

Remove old version, namely delete the `TFSAggregator.dll` and `AggregatorItems.xml` files from the plugin location on the Application Tier of your TFS Server.

The plugin folder is usually at this path: `C:\Program Files\Microsoft Team Foundation Server 12.0\Application Tier\Web Services\bin\Plugins`

At this point deploy the new version as described in [Install](Install.md).



## Convert the rules

Please refer [old syntax page](https://github.com/Vaccano/TFS-Aggregator/blob/master/docs/AggregatorItems-Syntax.md) and [new syntax](Policy-Syntax.md).

### Sample conversion

The old aggregation adds up the estimated work on the task.

```
<AggregatorItem operationType="Numeric" operation="Sum" linkType="Self" workItemType="Task">
    <TargetItem name="Estimated Work"/>
    <SourceItem name="Estimated Dev Work"/>
    <SourceItem name="Estimated Test Work"/>
</AggregatorItem>
```

The equivalent rule in the policy is

```
<rule name="Sum" appliesTo="Task" hasFields="Title,Description"><![CDATA[
    self["Estimated Work"] = (double)self["Estimated Dev Work"] + (double)self["Estimated Test Work"];
]]></rule>
```

Note the cast on fields' values.
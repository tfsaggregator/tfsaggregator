TFSAggregator2.ServerPlugin.policies syntax
================================================

A complete sample can be found at `..\UnitTests.Core\ConfigurationsForTests\syntax.xml`.

The XML Schema definition is in file `..\Aggregator.Core\Configuration\AggregatorConfiguration.xsd`.

```
<?xml version="1.0" encoding="utf-8"?>
```

This is the basic beginning to an XML file. Do not change it.

```
<AggregatorConfiguration>
```

**AggregatorConfiguration**: The main node for all the configuration options. (Single)

```
    <runtime debug="False">
```

**runtime**: Configure generic behavior. (Once, Optional)
 - **debug**: turns on debugging options (Optional, default: False)
 
```
        <logging level="Diagnostic"/>
```

**logging**: Define logging behavior. (Once, Optional)
 - **level**: The level of logging. (Optional)
Valid values are:
     * `Critical`
     * `Error`
     * `Warning`
     * `Information` or `Normal` -- default value
     * `Verbose`
     * `Diagnostic`.
See the Help page for more information: [TFS Aggregator Troubleshooting](Troubleshooting.md)

```
        <script language="C#" />
```

**script**: Define script engine behavior. (Once, Optional)
 - **language**: The language used to express the rules. (Optional)
Valid values are:
    * `CS`,`CSHARP`,`C#` -- default value
    * `VB`,`VB.NET`,`VBNET`
    * `PS`,`POWERSHELL` -- *Experimental*!

```
        <authentication autoImpersonate="true" />
```

**authentication**: Define authentication behavior. (Once, Optional)
 - **autoImpersonate**: `false` (default) the TFS Service account, `true` the user requesting. (Optional)


```
    <rule name="Noop" appliesTo="Task" hasFields="System.Title,System.Description">
    </rule>
```

**rule**: Represents a single aggregation rule. (Repeatable)

 - **name**: The name of this aggregation rule. (Mandatory)
 - **appliesTo**: The name of the work item type that this aggregation will target. (Optional)
 - **hasFields**: The work item must have the listed fields for the rule to apply. (List, Optional)
 - **_content_**: the script to execute when the rule triggers. (Mandatory)
   The `self` (`$self` in PowerShell) variable contains the work item that triggered the plugin.
   The `self` (`$self` in PowerShell) variable contains the work item that triggered the plugin.

We recommended using [CDATA](http://www.w3.org/TR/REC-xml/#sec-cdata-sect) to wrap script code.
See [Scripting](Scripting.md) for additional details.

```
    <policy name="DefaultPolicy">
```

**policy**: Represent a set of aggregation rules that apply to a particular scope. (Repeatable)

 - **name**: The name of this policy. (Mandatory)

All scopes must match for the policy to apply (logical _and_).

```
        <collectionScope collections="*" />
```

**collectionScope**: Scope the policy to a list of collections. (Optional)

 - **collections**: The TFS Collection to which the policy applies. (List, Mandatory)

```
        <templateScope name="" typeId="" minVersion="0.0" maxVersion="10.0" />
```

**templateScope**: Scope the policy to Team Projects using a particular Process Template. (Optional, Repeatable)

 - **name**: Name of Process Template matching. (Optional)
 - (Not working due to Microsoft not setting these values for on-premise installations)
  - **typeId**: Process Template GUID to match. (Optional)
  - **minVersion**: Minimum version for Process Template. (Optional)
  - **maxVersion**: Minimum version for Process Template. (Optional)

  Download the Process Template and look for the `metadata/version` node in `ProcessTemplate.xml` file to see matching values.

```
        <projectScope projects="Project1,Project2" />
```

**projectScope**: Scope the policy to listed Team Projects. (Optional)

 - **projects**: List of Team Project names. (List, Mandatory)

```
        <ruleRef name="Noop" />
```

**ruleRef**: Reference to a previously declared rule. (Repeatable)

 - **name**: Name of existing Rule. (Required)

Rules apply in order.

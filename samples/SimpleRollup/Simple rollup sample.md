# TFS Configuration
TFS must use a local or domain account: default *Local Service* won't work. The service account must be, at least, Project Admin.

# Test Project
No OOB Process templates support the sample.
Create a test project using CMMI template.
The install script will customize *Requirement* and *Task* work item types.

# Test Data
Requirement *Finish Date* must be set to a future date.
Set *Original Estimate* and *Remaining work* in a child task.

# Expected log
```
00000006	3.86630487	[8680] TFSAggregator: [Verbose]     Building Script Engine for C#
00000007	3.94802070	[8680] TFSAggregator: [Information] Starting processing on workitem #3
00000008	4.60871792	[8680] TFSAggregator: [Verbose]     Applying Policy 'DefaultPolicy'
00000009	4.61040211	[8680] TFSAggregator: [Verbose]     Evaluating Rule 'Rollup'
00000010	4.61381006	[8680] TFSAggregator: [Verbose]     Applying Rule 'Rollup' on #3
00000011	4.68550396	[8680] TFSAggregator: [Verbose]     Output from script 'Rollup': []
00000012	4.69083118	[8680] TFSAggregator: [Verbose]     Requirement [2] is valid to save.
00000013	4.91187954	[8680] TFSAggregator: [Information] Processing completed: Success
00000014	4.92762852	[8680] TFSAggregator: [Verbose]     Building Script Engine for C#
00000015	5.01021862	[8680] TFSAggregator: [Information] Starting processing on workitem #2
00000016	5.06257200	[8680] TFSAggregator: [Verbose]     Applying Policy 'DefaultPolicy'
00000017	5.06319141	[8680] TFSAggregator: [Verbose]     Evaluating Rule 'Rollup'
00000018	5.06371021	[8680] TFSAggregator: [Information] Processing completed: Success

```
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
[Verbose]     Output from script Rollup:
[Verbose]     Task [3] is valid to save.
[Verbose]     Requirement [2] is valid to save.
```
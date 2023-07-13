First and foremost thanks for considering.
The core team is made by very few people with limited time carved out of nights and weekends.


## Bug and Support request

Please spend some time reviewing the content of [Troubleshooting](https://github.com/tfsaggregator/tfsaggregator/wiki/Troubleshooting) page before submitting an Issue.

Label usage:
 - _bug_ bug in code
 - _documentation_ error in documentation
 - _todo_ something in the backlog that is not a feature, a bug or a document
 - _in progress_ work is on the way
 - _enhancement_ makes existing feature better
 - _feature_ a new feature that would be nice to add
 - _question_ request explanation or tips
 - _duplicate_ duplicate bug or question
 - _wontfix_ the core team will not consider fixing the bug (e.g. there is a reasonable workaround)
 - _Up for grabs_ the core team has no bandwidth for the proposal


## Code contribution

To compile and test:

- Install the version of Team Foundation Server or Azure DevOps Server you want to test with. It is not needed to configure the server to compile a build or run the unit tests.
- Open the solution in Visual Studio 2022.
- Select the project configuration that matches your version of the server.
- Compile and test the project from inside Visual Studio.

To test in a live server:

- Configure Team Foundation Server or Azure DevOps Server on your machine.
- Copy the dlls into the server's installation folders
- Attach the debugger to the TFS Job Agent service.
- Debug the solution.

To test from the cli

- Use the Console app generated to test a policy file without installing it into a server.

Submit a Pull Request with the changes.


## Documentation

Even non coders can help here. It is a public wiki that anyone can edit.

Documentation for a future release lives in a separate branch in the wiki repository:
the Wiki `master` branch content must always match the latest release.

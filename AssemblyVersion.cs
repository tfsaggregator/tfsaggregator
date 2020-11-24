using System.Reflection;

// To read Configuration use this Powershell snippet
// [System.Reflection.Assembly]::LoadFile("path-to-assembly-file").GetCustomAttributesData() | ?{ $_.AttributeType -eq [System.Reflection.AssemblyConfigurationAttribute] } | select ConstructorArguments
#if DEBUG
#if TFS2017
    [assembly: AssemblyConfiguration("Debug [TFS 2017.0 WebHooks]")]
#elif TFS2015u2
    [assembly: AssemblyConfiguration("Debug [TFS 2015.2]")]
#elif TFS2015u1
    [assembly: AssemblyConfiguration("Debug [TFS 2015.1]")]
#elif TFS2015
    [assembly: AssemblyConfiguration("Debug [TFS 2015.0]")]
#elif TFS2013
    [assembly: AssemblyConfiguration("Debug [TFS 2013.5]")]
#endif
#else
#if TFS2017
    [assembly: AssemblyConfiguration("Release [TFS 2017.0 WebHooks]")]
#elif TFS2015u2
    [assembly: AssemblyConfiguration("Release [TFS 2015.2]")]
#elif TFS2015u1
    [assembly: AssemblyConfiguration("Release [TFS 2015.1]")]
#elif TFS2015
    [assembly: AssemblyConfiguration("Release [TFS 2015.0]")]
#elif TFS2013
    [assembly: AssemblyConfiguration("Release [TFS 2013.5]")]
#endif
#endif
[assembly: AssemblyCompany("TFS Aggregator Team")]
[assembly: AssemblyProduct("TFS Aggregator")]
[assembly: AssemblyCopyright("Copyright (c) TFS Aggregator Team 2015-2020")]
[assembly: AssemblyTrademark("")]

// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version
//      Build Number
//      Revision
//
// You can specify all the values or you can default the Build and Revision Numbers
// by using the '*' as shown below:
// [assembly: AssemblyVersion("1.0.*")]
[assembly: AssemblyVersion("2.2.0.0")]
[assembly: AssemblyFileVersion("2.2.0.0")]
[assembly: AssemblyInformationalVersion("2.2-alpha")]

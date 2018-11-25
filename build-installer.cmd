rem "C:\Program Files (x86)\Microsoft Visual Studio\2017\Enterprise\MSBuild\15.0\Bin\msbuild.exe"
MSBuild build-installer.proj /m /p:Configuration=release /p:TF_BUILD=True /p:WixTargetsPath="C:\Program Files (x86)\MSBuild\Microsoft\WiX\v3.x\Wix.targets" /fl /flp:logfile=build-installer.log;verbosity=detailed /p:MajorMinorPatch="0.2.0" /p:FullSemVer="0.2.0-alpha+local" /bl

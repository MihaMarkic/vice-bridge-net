#load nuget:?package=Cake.Recipe&version=2.2.1

Environment.SetVariableNames();

BuildParameters.SetParameters(
    context: Context,
    buildSystem: BuildSystem,
    sourceDirectoryPath: "./source",
    title: "Righthand.Vice.Bridge",
    repositoryOwner: "MihaMarkic",
    repositoryName: "vice-bridge-net",
    appVeyorAccountName: "MihaMarkic",
	shouldRunDupFinder: false,
    shouldRunInspectCode: false,
	shouldRunCodecov: false,
    shouldRunDotNetCorePack: true);

BuildParameters.PrintParameters(Context);

ToolSettings.SetToolSettings(
    context: Context,
    dupFinderExcludePattern: new string[] { BuildParameters.RootDirectoryPath + "source/ViceMonitor.Bridge/Test/ViceMonitor.Bridge/*.cs" },
    testCoverageFilter: "+[*]* -[nunit.*]* -[*.Tests]*",
    testCoverageExcludeByAttribute: "*.ExcludeFromCodeCoverage*",
    testCoverageExcludeByFile: "*/*Designer.cs;*/*.g.cs;*/*.g.i.cs");
Build.RunDotNetCore();

var target = Argument("target", "Default");

var rootDir = Directory(".");
var solutionDir = rootDir + Directory("source/ViceMonitor.Bridge");
var solution = solutionDir + File("Righthand.ViceMonitor.Bridge.sln");

Task("Default")
	.Does(() => {
		var buildSettings = new DotNetBuildSettings
		{
			Configuration = "Release",
		};
        // first build with Playwright configuration
        // output will be also a Microsoft.Playwright assembly that can download browsers
        DotNetBuild(solution, buildSettings);
	});

RunTarget(target);
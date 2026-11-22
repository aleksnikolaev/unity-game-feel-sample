using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

/// <summary>
/// Headless builds. The WebGL one carries the decompression fallback on
/// purpose: a plain static host, GitHub Pages included, does not send the
/// Content-Encoding header Unity expects, and without the fallback the page
/// greets the visitor with a loader error instead of the scene.
/// </summary>
public static class BuildCommands
{
	static readonly string[] Scenes = { "Assets/Scenes/Range.unity" };

	[MenuItem("Range/Build WebGL")]
	public static void Web()
	{
		PlayerSettings.WebGL.decompressionFallback = true;
		PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Gzip;
		PlayerSettings.runInBackground = true;
		Run(BuildTarget.WebGL, BuildTargetGroup.WebGL, "Builds/web");
	}

	[MenuItem("Range/Build Mac")]
	public static void Mac()
	{
		Run(BuildTarget.StandaloneOSX, BuildTargetGroup.Standalone, "Builds/mac/RecoilRange.app");
	}

	[MenuItem("Range/Build Windows")]
	public static void Windows()
	{
		Run(BuildTarget.StandaloneWindows64, BuildTargetGroup.Standalone, "Builds/win/RecoilRange.exe");
	}

	static void Run(BuildTarget target, BuildTargetGroup group, string path)
	{
		PlayerSettings.companyName = "NIKSOFT";
		PlayerSettings.productName = "Recoil Range";

		BuildPlayerOptions options = new BuildPlayerOptions
		{
			scenes = Scenes,
			locationPathName = path,
			target = target,
			targetGroup = group,
			options = BuildOptions.None
		};

		BuildReport report = BuildPipeline.BuildPlayer(options);
		Debug.Log("RANGE BUILD " + target + " " + report.summary.result
			+ " bytes " + report.summary.totalSize
			+ " errors " + report.summary.totalErrors);
		EditorApplication.Exit(report.summary.result == BuildResult.Succeeded ? 0 : 1);
	}
}

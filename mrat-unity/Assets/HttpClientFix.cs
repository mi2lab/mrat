#if UNITY_EDITOR

using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
internal class HttpClientFix : AssetPostprocessor
{
	private static void OnGeneratedCSProjectFiles()
	{
//		Debug.Log("OnGeneratedCSProjectFiles, adding System.Net.Http reference");
		var dir = Directory.GetCurrentDirectory();
		var files = Directory.GetFiles(dir, "*.csproj");
		foreach (var file in files)
			FixProject(file);
	}

	private static bool FixProject(string file)
	{
		var text = File.ReadAllText(file);
		var find = "<Reference Include=\"System\" />";
		var replace = "<Reference Include=\"System\" /> <Reference Include=\"System.Net.Http\" />";
		if (text.IndexOf(find) != -1)
		{
			text = Regex.Replace(text, find, replace);
			File.WriteAllText(file, text);
			return true;
		}
		else
			return false;
	}
}
#endif

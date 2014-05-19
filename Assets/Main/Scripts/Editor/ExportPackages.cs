using UnityEngine;
using UnityEditor;

public class ExportPackages {
    [MenuItem("Assets/Build Package From Selection - Track dependencies")]
    static void ExportResource () {
        // Bring up save panel
        string path = EditorUtility.SaveFilePanel ("Save Resource", "", "New Resource", "package");
        if (path.Length != 0) {
            // Build the resource file from the active selection.
            Object[] selection = Selection.GetFiltered(typeof(Object), SelectionMode.DeepAssets);
            BuildPipeline.BuildAssetBundle(Selection.activeObject, selection, path, BuildAssetBundleOptions.CollectDependencies | BuildAssetBundleOptions.CompleteAssets);
            Selection.objects = selection;
        }
    }
    [MenuItem("Assets/Build Package From Selection - No dependency tracking")]
    static void ExportResourceNoTrack () {
        // Bring up save panel
        string path = EditorUtility.SaveFilePanel ("Save Resource", "", "New Resource", "package");
        if (path.Length != 0) {
            // Build the resource file from the active selection.
            BuildPipeline.BuildAssetBundle(Selection.activeObject, Selection.objects, path);
        }
    }
	
	[MenuItem("Assets/Build map")]
	static void ExportMap () {
		string[] levels = new string[1]{"Assets/Main/Levels/Network/TestLevelDev.unity"};
		string path = EditorUtility.SaveFilePanel ("Save Resource", "", "New Resource", "map");
        if (path.Length != 0) {
	  		BuildPipeline.BuildStreamedSceneAssetBundle(levels, path, BuildTarget.WebPlayer);
		}
	}
	
	[MenuItem("Assets/Fast Build Map 1")]
	static void FastExportMap1 () {
		string[] levels = new string[1]{"Assets/Main/Levels/Level1/Streamed.unity"};
		BuildPipeline.BuildStreamedSceneAssetBundle(levels, "Packages/Maps/TestLevel_1.map", BuildTarget.StandaloneWindows);
	}
	
	[MenuItem("Assets/Fast Build Map 2")]
	static void FastExportMap2 () {
		string[] levels = new string[1]{"Assets/Main/Levels/Level2/Streamed.unity"};
		BuildPipeline.BuildStreamedSceneAssetBundle(levels, "Packages/Maps/TestLevel_2.map", BuildTarget.StandaloneWindows);
	}
}

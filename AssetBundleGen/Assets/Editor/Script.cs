using UnityEditor;
using System.IO;

public class CreateAssetBundles
{
    [MenuItem("Tools/Build AssetBundles")]
    static void BuildAllAssetBundles()
    {
        string assetBundleDirectory = "../Essentials/Assets/";
        if(!Directory.Exists(assetBundleDirectory))
        {
            Directory.CreateDirectory(assetBundleDirectory);
        }
        BuildPipeline.BuildAssetBundles(assetBundleDirectory,
                                        BuildAssetBundleOptions.None,
                                        EditorUserBuildSettings.activeBuildTarget);
        File.Delete(assetBundleDirectory+"starlight.bundle.manifest");
        File.Delete(assetBundleDirectory+"Assets.manifest");
        File.Delete(assetBundleDirectory+"Assets");
    }
}
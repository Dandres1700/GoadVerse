#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

public class HistoriaStreamingAssetsBuildProcessor : IPreprocessBuildWithReport
{
    public int callbackOrder => 0;

    public void OnPreprocessBuild(BuildReport report)
    {
        SyncHistoriaAssets(false);
    }

    [MenuItem("GoadVerse/Sync Historia Streaming Assets")]
    public static void SyncHistoriaAssets()
    {
        SyncHistoriaAssets(true);
    }

    private static void SyncHistoriaAssets(bool refreshAssetDatabase)
    {
        SceneUtilityUISettings settings = SceneUtilityUISettings.Get();
        string historiaFolderName = settings.historiaFolderName;
        string sourceFolder = Path.Combine(Application.dataPath, historiaFolderName);
        string targetFolder = Path.Combine(Application.streamingAssetsPath, historiaFolderName);

        if (!Directory.Exists(sourceFolder))
        {
            Debug.LogWarning("No se encontro Assets/" + historiaFolderName + ". El intro de historia no se copiara al build.");
            return;
        }

        Directory.CreateDirectory(targetFolder);

        foreach (string sourceFile in Directory.GetFiles(sourceFolder))
        {
            if (sourceFile.EndsWith(".meta"))
            {
                continue;
            }

            string extension = Path.GetExtension(sourceFile).ToLowerInvariant();

            if (!IsHistoriaRuntimeFile(extension))
            {
                continue;
            }

            if (new FileInfo(sourceFile).Length <= 0)
            {
                Debug.LogWarning("El archivo de historia esta vacio y no se copiara: " + sourceFile);
                continue;
            }

            string targetFile = Path.Combine(targetFolder, Path.GetFileName(sourceFile));
            File.Copy(sourceFile, targetFile, true);
        }

        if (refreshAssetDatabase)
        {
            AssetDatabase.Refresh();
        }

        Debug.Log("Historia copiada a StreamingAssets para el build.");
    }

    private static bool IsHistoriaRuntimeFile(string extension)
    {
        return extension == ".mp4"
            || extension == ".mov"
            || extension == ".webm"
            || extension == ".avi"
            || extension == ".mp3"
            || extension == ".wav"
            || extension == ".ogg";
    }
}
#endif

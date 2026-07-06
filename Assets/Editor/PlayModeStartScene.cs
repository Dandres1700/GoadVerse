#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;

[InitializeOnLoad]
public static class PlayModeStartScene
{
    private const string BootstrapScenePath = "Assets/Scenes/Bootstrap.unity";

    static PlayModeStartScene()
    {
        SceneAsset bootstrapScene = AssetDatabase.LoadAssetAtPath<SceneAsset>(BootstrapScenePath);
        if (bootstrapScene == null)
            return;

        if (EditorSceneManager.playModeStartScene != bootstrapScene)
        {
            EditorSceneManager.playModeStartScene = bootstrapScene;
        }
    }
}
#endif

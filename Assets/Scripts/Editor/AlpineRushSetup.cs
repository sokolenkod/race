using UnityEditor;

namespace AlpineRush.Editor
{
    [InitializeOnLoad]
    public static class AlpineRushSetup
    {
        const string MenuPath = "Assets/Scenes/MainMenu.unity";
        const string RacePath = "Assets/Scenes/Race.unity";

        static AlpineRushSetup()
        {
            EditorApplication.delayCall += EnsureBuildSettings;
        }

        [MenuItem("Alpine Rush/Ensure Scenes In Build")]
        public static void EnsureBuildSettings()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            var scenes = EditorBuildSettings.scenes;
            bool ok = scenes.Length >= 2
                && scenes[0].enabled && scenes[0].path == MenuPath
                && scenes[1].enabled && scenes[1].path == RacePath;
            if (ok) return;
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(MenuPath) == null) return;
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(RacePath) == null) return;

            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene(MenuPath, true),
                new EditorBuildSettingsScene(RacePath, true)
            };
        }
    }
}

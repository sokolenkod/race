using UnityEngine;
using UnityEngine.SceneManagement;

namespace AlpineRush
{
    public static class SampleSceneRedirect
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void RedirectTemplateScene()
        {
            var scene = SceneManager.GetActiveScene();
            if (scene.name != "SampleScene") return;
            if (Application.CanStreamedLevelBeLoaded(GameFlow.MenuScene))
                SceneManager.LoadScene(GameFlow.MenuScene);
        }
    }
}

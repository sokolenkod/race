using UnityEngine;
using UnityEngine.SceneManagement;

namespace AlpineRush
{
    public static class GameFlow
    {
        public const string MenuScene = "MainMenu";
        public const string RaceScene = "Race";

        public static void StartRace()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(RaceScene);
        }

        public static void MainMenu()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(MenuScene);
        }

        public static void RestartRace()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(RaceScene);
        }

        public static void Quit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}

using UnityEngine;
using UnityEngine.SceneManagement;

public static class SceneLoader 
{
    public enum Scene
    {
        MainMenuScene,
        GameScene,
        GameOverScene,
        LanderSelectionScene,
        LevelSelectionScene,
    }
    public static void LoadScene(Scene scene)
    {
        SceneManager.LoadScene(scene.ToString());
    }

    public static void LoadScene(string sceneName)
    {
        // Use fade transition if available
        if (SceneTransition.Instance != null)
        {
            SceneTransition.Instance.FadeToScene(sceneName);
        }
        else
        {
            // Fallback: Load scene directly
            SceneManager.LoadScene(sceneName);
        }
    }

    private static string GetSceneName(Scene scene)
    {
        switch (scene)
        {
            default:
            case Scene.MainMenuScene:
                return "MainMenuScene";
            case Scene.GameScene:
                return "GameScene";
            case Scene.GameOverScene:
                return "GameOverScene";
            case Scene.LevelSelectionScene: 
                return "LevelSelectionScene";
        }
    }
}

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
}

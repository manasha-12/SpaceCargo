using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour
{
    [SerializeField] private Button playButton;
    [SerializeField] private Button selectLanderButton;
    [SerializeField] private Button quitButton;

    private void Awake()
    {
        Time.timeScale = 1f;

        playButton.onClick.AddListener(() => {
            GameManager.ResetStaticData();
            SceneLoader.LoadScene(SceneLoader.Scene.LevelSelectionScene);
        });

        selectLanderButton.onClick.AddListener(() => {
            SceneLoader.LoadScene(SceneLoader.Scene.LanderSelectionScene);
        });

        quitButton.onClick.AddListener(() => {
            Application.Quit();
        });
    }

    private void Start()
    {
        // Deselect everything immediately so held Submit from previous
        // scene doesn't auto-fire the focused button
        EventSystem.current.SetSelectedGameObject(null);

        // Re-select after 2 frames once input state has cleared
        StartCoroutine(SelectAfterDelay());
    }

    private IEnumerator SelectAfterDelay()
    {
        yield return null; // wait 1 frame
        yield return null; // wait 1 more frame to be safe

        if (EventSystem.current != null && playButton != null)
        {
            EventSystem.current.SetSelectedGameObject(playButton.gameObject);
        }
    }
}
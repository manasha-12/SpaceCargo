using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour
{
    [SerializeField] private Button playButton;
    [SerializeField] private Button selectLanderButton;
    [SerializeField] private Button quitButton;
    [SerializeField] private Button leadersButton;
    [SerializeField] private LeaderboardUI leaderboardUI;

    private bool isTransitioning = false;

    private void Awake()
    {
        Time.timeScale = 1f;

        playButton.onClick.AddListener(() =>
        {
            if (isTransitioning) return;
            isTransitioning = true;
            GameManager.ResetStaticData();
            StartCoroutine(LoadNameScreenAfterDelay());
        });

        selectLanderButton.onClick.AddListener(() =>
        {
            if (isTransitioning) return;
            isTransitioning = true;
            SceneLoader.LoadScene(SceneLoader.Scene.LanderSelectionScene);
        });

        quitButton.onClick.AddListener(() =>
        {
            Application.Quit();
        });

        if (leadersButton != null && leaderboardUI != null)
            leadersButton.onClick.AddListener(() => leaderboardUI.Show());
    }

    private IEnumerator LoadNameScreenAfterDelay()
    {
        yield return new WaitForSecondsRealtime(0.3f);
        SceneLoader.LoadScene(SceneLoader.Scene.PlayerNameScene);
    }

    private void Start()
    {
        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(null);

        if (GameInput.Instance != null)
            GameInput.Instance.DisableSubmitAction();

        StartCoroutine(SelectAfterDelay());
    }

    private IEnumerator SelectAfterDelay()
    {
        // 0.5s — longer than 0.3s to prevent input bleed in builds
        // In builds, scene loading takes extra frames so the controller
        // button can still be "held" when the new scene's EventSystem initialises
        yield return new WaitForSecondsRealtime(0.5f);

        if (GameInput.Instance != null)
            GameInput.Instance.EnableSubmitAction();

        if (EventSystem.current != null && playButton != null)
            EventSystem.current.SetSelectedGameObject(playButton.gameObject);
    }
}
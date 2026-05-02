using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class PausedUI : MonoBehaviour
{
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button mainMenuButton;

    private CanvasGroup canvasGroup;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();

        HideImmediate();

        if (resumeButton != null)
        {
            resumeButton.onClick.AddListener(() =>
            {
                if (GameManager.Instance != null)
                    GameManager.Instance.UnPauseGame();
            });
        }

        if (mainMenuButton != null)
        {
            mainMenuButton.onClick.AddListener(() =>
            {
                // Deselect and disable submit BEFORE loading scene
                if (EventSystem.current != null)
                    EventSystem.current.SetSelectedGameObject(null);

                if (GameInput.Instance != null)
                    GameInput.Instance.DisableSubmitAction();

                Time.timeScale = 1f;

                StartCoroutine(LoadMainMenuAfterDelay());
            });
        }
    }

    private IEnumerator LoadMainMenuAfterDelay()
    {
        // Time-based delay so Cross release is registered before new scene loads
        yield return new WaitForSecondsRealtime(0.3f);
        SceneLoader.LoadScene(SceneLoader.Scene.MainMenuScene);
    }

    private void Start()
    {
        if (GameManager.Instance == null) return;

        GameManager.Instance.OnGamePaused -= GameManager_OnGamePaused;
        GameManager.Instance.OnGameUnPaused -= GameManager_OnGameUnPaused;
        GameManager.Instance.OnGamePaused += GameManager_OnGamePaused;
        GameManager.Instance.OnGameUnPaused += GameManager_OnGameUnPaused;
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGamePaused -= GameManager_OnGamePaused;
            GameManager.Instance.OnGameUnPaused -= GameManager_OnGameUnPaused;
        }
    }

    private void GameManager_OnGamePaused(object sender, System.EventArgs e) => Show();
    private void GameManager_OnGameUnPaused(object sender, System.EventArgs e) => Hide();

    private void Show()
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }

        StartCoroutine(SelectResumeAfterDelay());
    }

    private IEnumerator SelectResumeAfterDelay()
    {
        yield return new WaitForSecondsRealtime(0.1f);
        if (resumeButton != null && EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(resumeButton.gameObject);
    }

    private void Hide()
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }

        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(null);
    }

    private void HideImmediate()
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }
    }
}
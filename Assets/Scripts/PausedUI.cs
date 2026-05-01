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
        // Get or add CanvasGroup
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
            Debug.Log("PausedUI: Added CanvasGroup component");
        }

        // IMPORTANT: Don't disable GameObject, just hide visually
        HideImmediate();

        // Setup button listeners
        if (resumeButton != null)
        {
            resumeButton.onClick.AddListener(() =>
            {
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.UnPauseGame();
                }
            });
        }

        if (mainMenuButton != null)
        {
            mainMenuButton.onClick.AddListener(() =>
            {
                Time.timeScale = 1f;
                SceneLoader.LoadScene(SceneLoader.Scene.MainMenuScene);
            });
        }

        Debug.Log("PausedUI: Awake completed");
    }

    private void Start()
    {
        Debug.Log("PausedUI: Start() called");

        if (GameManager.Instance == null)
        {
            Debug.LogWarning("PausedUI: GameManager not found!");
            return;
        }

        // Unsubscribe first to prevent duplicates
        GameManager.Instance.OnGamePaused -= GameManager_OnGamePaused;
        GameManager.Instance.OnGameUnPaused -= GameManager_OnGameUnPaused;

        // Subscribe to events
        GameManager.Instance.OnGamePaused += GameManager_OnGamePaused;
        GameManager.Instance.OnGameUnPaused += GameManager_OnGameUnPaused;

        Debug.Log("PausedUI: Successfully subscribed to GameManager events");
    }

    private void OnDestroy()
    {
        Debug.Log("PausedUI: OnDestroy() called");

        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGamePaused -= GameManager_OnGamePaused;
            GameManager.Instance.OnGameUnPaused -= GameManager_OnGameUnPaused;
        }
    }

    private void GameManager_OnGamePaused(object sender, System.EventArgs e)
    {
        Debug.Log("PausedUI: *** PAUSE EVENT RECEIVED ***");
        Show();
    }

    private void GameManager_OnGameUnPaused(object sender, System.EventArgs e)
    {
        Debug.Log("PausedUI: *** UNPAUSE EVENT RECEIVED ***");
        Hide();
    }

    private void Show()
    {
        Debug.Log("PausedUI: Show() - Making UI visible");

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }

        // Select resume button for keyboard navigation
        if (resumeButton != null)
        {
            resumeButton.Select();
        }

        Debug.Log($"PausedUI: Canvas Group Alpha = {canvasGroup?.alpha}");
    }

    private void Hide()
    {
        Debug.Log("PausedUI: Hide() - Making UI invisible");

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }

        // Deselect all UI
        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
        }
    }

    private void HideImmediate()
    {
        // Hide without logging (called in Awake)
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }
    }
}
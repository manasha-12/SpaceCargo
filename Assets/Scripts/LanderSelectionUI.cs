using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class LanderSelectionUI : MonoBehaviour
{
    [Header("Lander Buttons")]
    [SerializeField] private Button lander1Button;
    [SerializeField] private Button lander2Button;
    [SerializeField] private Button lander3Button;

    [Header("Selection Borders (Optional)")]
    [SerializeField] private GameObject lander1Border;
    [SerializeField] private GameObject lander2Border;
    [SerializeField] private GameObject lander3Border;

    [Header("Action Buttons")]
    [SerializeField] private Button playButton;
    [SerializeField] private Button backButton;

    private bool isTransitioning = false;

    // Debounce: track last selection time to prevent double-fire from controller
    private float lastSelectionTime = -1f;
    private const float SELECTION_COOLDOWN = 0.3f;

    private void Awake()
    {

        playButton.onClick.AddListener(() =>
        {
            if (isTransitioning) return;
            isTransitioning = true;

            if (EventSystem.current != null)
                EventSystem.current.SetSelectedGameObject(null);

            if (GameInput.Instance != null)
                GameInput.Instance.DisableSubmitAction();

            StartCoroutine(ConfirmAndPlayAfterDelay());
        });

        backButton.onClick.AddListener(() =>
        {
            if (isTransitioning) return;
            isTransitioning = true;

            if (EventSystem.current != null)
                EventSystem.current.SetSelectedGameObject(null);

            if (GameInput.Instance != null)
                GameInput.Instance.DisableSubmitAction();

            StartCoroutine(LoadMainMenuAfterDelay());
        });
    }

    private void Start()
    {
        // Clear any held input from previous scene immediately
        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(null);

        // Disable submit so held Cross from previous scene doesn't fire
        if (GameInput.Instance != null)
            GameInput.Instance.DisableSubmitAction();

        UpdateSelectionVisual(LanderSelectionManager.Instance.GetSelectedLander());

        StartCoroutine(SelectDefaultButton());
    }

    private IEnumerator SelectDefaultButton()
    {
        yield return new WaitForSecondsRealtime(0.3f);

        // Re-enable submit and reset debounce AFTER delay
        if (GameInput.Instance != null)
            GameInput.Instance.EnableSubmitAction();

        // Reset selection time so first controller press isn't blocked
        lastSelectionTime = -1f;

        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(lander1Button.gameObject);
    }

    private IEnumerator ConfirmAndPlayAfterDelay()
    {
        yield return new WaitForSecondsRealtime(0.2f);
        LanderSelectionManager.Instance.ConfirmSelectionAndPlay();
    }

    private IEnumerator LoadMainMenuAfterDelay()
    {
        yield return new WaitForSecondsRealtime(0.3f);
        LanderSelectionManager.Instance.BackToMainMenu();
    }

    public void SelectLanderPublic(LanderSelectionManager.LanderType landerType)
    {
        if (isTransitioning) return;

        // Debounce: ignore if called too soon after last selection
        // This prevents controller from firing onClick twice in one press
        if (Time.unscaledTime - lastSelectionTime < SELECTION_COOLDOWN)
        {
            Debug.Log("LanderSelectionUI: Selection debounced (double-fire prevented)");
            return;
        }

        lastSelectionTime = Time.unscaledTime;

        LanderSelectionManager.Instance.SelectLander(landerType);
        UpdateSelectionVisual(landerType);
    }

    private void UpdateSelectionVisual(LanderSelectionManager.LanderType landerType)
    {
        if (lander1Border != null) lander1Border.SetActive(false);
        if (lander2Border != null) lander2Border.SetActive(false);
        if (lander3Border != null) lander3Border.SetActive(false);

        switch (landerType)
        {
            case LanderSelectionManager.LanderType.Lander1:
                if (lander1Border != null) lander1Border.SetActive(true);
                break;
            case LanderSelectionManager.LanderType.Lander2:
                if (lander2Border != null) lander2Border.SetActive(true);
                break;
            case LanderSelectionManager.LanderType.Lander3:
                if (lander3Border != null) lander3Border.SetActive(true);
                break;
        }
    }
}
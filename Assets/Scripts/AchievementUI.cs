using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class AchievementUI : MonoBehaviour
{
    public static AchievementUI Instance { get; private set; }

    [Header("Pre-Level Panel")]
    [SerializeField] private GameObject preLevelPanel;
    [SerializeField] private Transform preLevelContainer;
    [SerializeField] private GameObject achievementRowPrefab;
    [SerializeField] private TextMeshProUGUI preLevelTitleText;

    [Header("Post-Landing Panel")]
    [SerializeField] private GameObject postLandingPanel;
    [SerializeField] private Transform postLandingContainer;
    [SerializeField] private TextMeshProUGUI starsEarnedText;
    [SerializeField] private Transform starsContainer;
    [SerializeField] private Button postContinueButton;

    private CanvasGroup postLandingCanvasGroup;
    private CanvasGroup preLevelCanvasGroup;
    private System.Action onContinueCallback;

    private void Awake()
    {
        Instance = this;

        if (postLandingPanel != null)
        {
            postLandingCanvasGroup = postLandingPanel.GetComponent<CanvasGroup>();
            if (postLandingCanvasGroup == null)
                postLandingCanvasGroup = postLandingPanel.AddComponent<CanvasGroup>();
            // Force hide immediately
            postLandingPanel.SetActive(false);
            postLandingCanvasGroup.alpha = 0f;
            postLandingCanvasGroup.interactable = false;
            postLandingCanvasGroup.blocksRaycasts = false;
        }

        if (preLevelPanel != null)
        {
            preLevelCanvasGroup = preLevelPanel.GetComponent<CanvasGroup>();
            if (preLevelCanvasGroup == null)
                preLevelCanvasGroup = preLevelPanel.AddComponent<CanvasGroup>();
            preLevelPanel.SetActive(false);
            preLevelCanvasGroup.alpha = 0f;
            preLevelCanvasGroup.interactable = false;
            preLevelCanvasGroup.blocksRaycasts = false;
        }
    }

    private void Start()
    {
        if (postContinueButton != null)
            postContinueButton.onClick.AddListener(OnContinueClicked);
        else
            Debug.LogWarning("AchievementUI: postContinueButton is NOT assigned in Inspector!");
    }

    // ── Pre-Level ─────────────────────────────────────────────────────────
    public void ShowPreLevelAchievements(int levelNumber, System.Action onDismiss)
    {
        if (preLevelPanel == null || AchievementManager.Instance == null) return;

        var achievements = AchievementManager.Instance.GetAchievementsForLevel(levelNumber);

        if (preLevelTitleText != null)
            preLevelTitleText.text = $"LEVEL {levelNumber} CHALLENGES";

        if (preLevelContainer != null)
        {
            foreach (Transform child in preLevelContainer)
                Destroy(child.gameObject);

            foreach (var a in achievements)
            {
                if (achievementRowPrefab == null) break;
                var row = Instantiate(achievementRowPrefab, preLevelContainer);
                SetupRow(row, a);
            }
        }

        preLevelPanel.SetActive(true);
        if (preLevelCanvasGroup != null)
        {
            preLevelCanvasGroup.alpha = 1f;
            preLevelCanvasGroup.interactable = true;
            preLevelCanvasGroup.blocksRaycasts = true;
        }
    }

    public void HidePreLevel()
    {
        if (preLevelPanel == null) return;
        preLevelPanel.SetActive(false);
        if (preLevelCanvasGroup != null)
        {
            preLevelCanvasGroup.alpha = 0f;
            preLevelCanvasGroup.interactable = false;
            preLevelCanvasGroup.blocksRaycasts = false;
        }
    }

    // ── Post-Landing ──────────────────────────────────────────────────────
    public void ShowPostLandingAchievements(List<Achievement> newlyCompleted,
        int totalStars, System.Action onContinue)
    {
        Debug.Log($"ShowPostLandingAchievements called. postLandingPanel null? {postLandingPanel == null}");
        Debug.Log($"postLandingContainer null? {postLandingContainer == null}");
        Debug.Log($"postContinueButton null? {postContinueButton == null}");

        // SAFETY: if anything critical is missing, call onContinue immediately
        if (postLandingPanel == null || postLandingContainer == null || postContinueButton == null)
        {
            Debug.LogWarning("AchievementUI: Missing references — skipping achievement panel, calling onContinue directly");
            onContinue?.Invoke();
            return;
        }

        onContinueCallback = onContinue;

        // Safe clear
        foreach (Transform child in postLandingContainer)
            Destroy(child.gameObject);

        // Set text
        if (newlyCompleted == null || newlyCompleted.Count == 0)
        {
            if (starsEarnedText != null)
                starsEarnedText.text = "No new achievements this run";
        }
        else
        {
            if (starsEarnedText != null)
                starsEarnedText.text = $"+{newlyCompleted.Count} star(s) earned!";

            foreach (var a in newlyCompleted)
            {
                if (achievementRowPrefab == null) break;
                var row = Instantiate(achievementRowPrefab, postLandingContainer);
                SetupRow(row, a);
            }
        }

        UpdateStarDisplay(newlyCompleted?.Count ?? 0);

        // Show panel
        postLandingPanel.SetActive(true);
        if (postLandingCanvasGroup != null)
        {
            postLandingCanvasGroup.alpha = 1f;
            postLandingCanvasGroup.interactable = true;
            postLandingCanvasGroup.blocksRaycasts = true;
        }

        Debug.Log("AchievementUI: postLandingPanel shown, waiting for CONTINUE click");
        StartCoroutine(SelectContinueAfterDelay());
    }

    private IEnumerator SelectContinueAfterDelay()
    {
        yield return new WaitForSecondsRealtime(0.15f);
        if (postContinueButton != null && EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(postContinueButton.gameObject);
            Debug.Log("AchievementUI: CONTINUE button selected for controller");
        }
    }

    private void OnContinueClicked()
    {
        Debug.Log("AchievementUI: CONTINUE clicked — hiding panel and calling callback");

        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(null);

        // Hide
        postLandingPanel.SetActive(false);
        if (postLandingCanvasGroup != null)
        {
            postLandingCanvasGroup.alpha = 0f;
            postLandingCanvasGroup.interactable = false;
            postLandingCanvasGroup.blocksRaycasts = false;
        }

        StartCoroutine(CallbackAfterDelay());
    }

    private IEnumerator CallbackAfterDelay()
    {
        yield return new WaitForSecondsRealtime(0.1f);
        Debug.Log("AchievementUI: Invoking onContinueCallback");
        onContinueCallback?.Invoke();
    }

    private void UpdateStarDisplay(int newStars)
    {
        if (starsContainer == null) return;
        for (int i = 0; i < starsContainer.childCount && i < 3; i++)
        {
            var img = starsContainer.GetChild(i).GetComponent<Image>();
            if (img == null) continue;
            img.color = i < newStars
                ? new Color(1f, 0.84f, 0f)
                : new Color(0.3f, 0.3f, 0.3f);
        }
    }

    private void SetupRow(GameObject row, Achievement a)
    {
        if (row == null) return;

        var titleText = row.transform.Find("TitleText")?.GetComponent<TextMeshProUGUI>();
        var descText = row.transform.Find("DescText")?.GetComponent<TextMeshProUGUI>();
        var starIcon = row.transform.Find("StarIcon")?.GetComponent<Image>();
        var checkGO = row.transform.Find("CheckIcon")?.gameObject;

        if (titleText != null) titleText.text = a.title;
        if (descText != null) descText.text = a.description;

        if (starIcon != null)
            starIcon.color = a.isCompleted
                ? new Color(1f, 0.84f, 0f)
                : new Color(0.4f, 0.4f, 0.4f);

        if (checkGO != null)
            checkGO.SetActive(a.isCompleted);

        var bg = row.GetComponent<Image>();
        if (bg != null)
            bg.color = a.isCompleted
                ? new Color(0.05f, 0.25f, 0.05f, 0.9f)
                : new Color(0.08f, 0.08f, 0.18f, 0.9f);
    }
}
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class LeaderboardUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform entriesContainer;
    [SerializeField] private GameObject entryRowPrefab;
    [SerializeField] private Button closeButton;
    [SerializeField] private GameObject panelRoot;

    [Header("Button to re-select after closing (for controller)")]
    [SerializeField] private Button buttonToSelectOnClose;

    // A persistent runner that is NEVER deactivated so coroutines survive Hide()
    private static GameObject _runner;
    private static MonoBehaviour Runner
    {
        get
        {
            if (_runner == null)
            {
                _runner = new GameObject("LeaderboardCoroutineRunner");
                DontDestroyOnLoad(_runner);
                _runner.AddComponent<LeaderboardCoroutineRunner>();
            }
            return _runner.GetComponent<LeaderboardCoroutineRunner>();
        }
    }

    private void Awake()
    {
        if (closeButton != null)
            closeButton.onClick.AddListener(OnCloseClicked);
    }

    private void Start()
    {
        if (panelRoot != null)
            panelRoot.SetActive(false);
    }

    private void OnCloseClicked()
    {
        // 1. Deselect immediately — removes the EventSystem reference to CloseButton
        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(null);

        // 2. Run the hide + reselect on the PERSISTENT runner, not on this component
        // This survives panelRoot.SetActive(false) which would kill coroutines on this MB
        Runner.StartCoroutine(HideAndReselect());
    }

    private IEnumerator HideAndReselect()
    {
        // Wait for controller button release
        yield return new WaitForSecondsRealtime(0.2f);

        // Hide the panel
        Hide();

        // Wait one more frame for EventSystem to settle
        yield return new WaitForSecondsRealtime(0.05f);

        // Re-enable submit in case it was disabled
        if (GameInput.Instance != null)
            GameInput.Instance.EnableSubmitAction();

        // Re-select the button so controller navigation works again
        if (EventSystem.current != null && buttonToSelectOnClose != null)
            EventSystem.current.SetSelectedGameObject(buttonToSelectOnClose.gameObject);
    }

    public void Show()
    {
        if (panelRoot != null)
            panelRoot.SetActive(true);

        DisplayLeaderboard();

        // Select close button for controller after panel opens
        Runner.StartCoroutine(SelectClose());
    }

    private IEnumerator SelectClose()
    {
        yield return new WaitForSecondsRealtime(0.15f);
        if (EventSystem.current != null && closeButton != null)
            EventSystem.current.SetSelectedGameObject(closeButton.gameObject);
    }

    public void Hide()
    {
        if (panelRoot != null)
            panelRoot.SetActive(false);
    }

    public void DisplayLeaderboard()
    {
        if (LeaderboardManager.Instance == null) return;

        foreach (Transform child in entriesContainer)
            Destroy(child.gameObject);

        List<ScoreEntry> entries = LeaderboardManager.Instance.GetTopScores();

        for (int i = 0; i < entries.Count; i++)
        {
            GameObject row = Instantiate(entryRowPrefab, entriesContainer);

            TextMeshProUGUI rankText = row.transform.Find("RankText")?.GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI nameText = row.transform.Find("NameText")?.GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI scoreText = row.transform.Find("ScoreText")?.GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI dateText = row.transform.Find("DateText")?.GetComponent<TextMeshProUGUI>();

            if (rankText != null) rankText.text = $"#{i + 1}";
            if (nameText != null) nameText.text = entries[i].playerName;
            if (scoreText != null) scoreText.text = entries[i].score.ToString();
            if (dateText != null) dateText.text = entries[i].date;

            if (i == 0)
            {
                Image rowImage = row.GetComponent<Image>();
                if (rowImage != null)
                    rowImage.color = new Color(1f, 0.84f, 0f, 0.3f);
            }
        }

        if (entries.Count == 0)
        {
            GameObject row = Instantiate(entryRowPrefab, entriesContainer);
            TextMeshProUGUI nameText = row.transform.Find("NameText")?.GetComponent<TextMeshProUGUI>();
            if (nameText != null)
                nameText.text = "No scores yet — play to get on the board!";
        }
    }
}

// Minimal persistent MonoBehaviour used only to run coroutines that must
// survive the LeaderboardPanel being deactivated
public class LeaderboardCoroutineRunner : MonoBehaviour { }
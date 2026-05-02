using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LeaderboardUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform entriesContainer;
    [SerializeField] private GameObject entryRowPrefab;
    [SerializeField] private Button closeButton;
    [SerializeField] private GameObject panelRoot;

    private void Awake()
    {
        if (closeButton != null)
            closeButton.onClick.AddListener(Hide);
    }

    private void Start()
    {
        // In GameOver scene, show automatically
        // In MainMenu, stays hidden until button pressed
        if (panelRoot != null)
            panelRoot.SetActive(false);
    }

    public void Show()
    {
        if (panelRoot != null)
            panelRoot.SetActive(true);
        DisplayLeaderboard();
    }

    public void Hide()
    {
        if (panelRoot != null)
            panelRoot.SetActive(false);
    }

    public void DisplayLeaderboard()
    {
        if (LeaderboardManager.Instance == null) return;

        // Clear old entries
        foreach (Transform child in entriesContainer)
            Destroy(child.gameObject);

        List<ScoreEntry> entries = LeaderboardManager.Instance.GetTopScores();

        for (int i = 0; i < entries.Count; i++)
        {
            GameObject row = Instantiate(entryRowPrefab, entriesContainer);

            // Find text components by name
            TextMeshProUGUI rankText = row.transform.Find("RankText")?.GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI nameText = row.transform.Find("NameText")?.GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI scoreText = row.transform.Find("ScoreText")?.GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI dateText = row.transform.Find("DateText")?.GetComponent<TextMeshProUGUI>();

            if (rankText != null) rankText.text = $"#{i + 1}";
            if (nameText != null) nameText.text = entries[i].playerName;
            if (scoreText != null) scoreText.text = entries[i].score.ToString();
            if (dateText != null) dateText.text = entries[i].date;

            // Highlight top entry gold
            if (i == 0)
            {
                Image rowImage = row.GetComponent<Image>();
                if (rowImage != null)
                    rowImage.color = new Color(1f, 0.84f, 0f, 0.3f);
            }
        }

        // Show "No scores yet" if empty
        if (entries.Count == 0)
        {
            GameObject row = Instantiate(entryRowPrefab, entriesContainer);
            TextMeshProUGUI nameText = row.transform.Find("NameText")?.GetComponent<TextMeshProUGUI>();
            if (nameText != null) nameText.text = "No scores yet — play to get on the board!";
        }
    }
}
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LeaderboardUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject leaderboardEntryPrefab;
    [SerializeField] private Transform leaderboardContainer;
    [SerializeField] private TextMeshProUGUI titleText;

    [Header("Colors")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color highlightColor = Color.yellow;

    private int currentPlayerScore = -1;

    private void Start()
    {
        DisplayLeaderboard();
    }

    public void DisplayLeaderboard(int highlightScore = -1)
    {
        currentPlayerScore = highlightScore;

        // Clear existing entries
        foreach (Transform child in leaderboardContainer)
        {
            Destroy(child.gameObject);
        }

        // Check if LeaderboardManager exists
        if (LeaderboardManager.Instance == null)
        {
            Debug.LogError("LeaderboardUI: LeaderboardManager.Instance is NULL!");
            // Show 5 empty entries
            for (int i = 1; i <= 5; i++)
            {
                CreateEmptyEntry(i);
            }
            return;
        }

        // Get leaderboard data
        List<LeaderboardEntry> entries = LeaderboardManager.Instance.GetLeaderboard();

        Debug.Log($"Leaderboard has {entries.Count} entries");

        // Create UI entry for each leaderboard entry
        for (int i = 0; i < entries.Count; i++)
        {
            CreateLeaderboardEntry(i + 1, entries[i]);
        }

        // If leaderboard has less than 5 entries, show empty slots
        for (int i = entries.Count; i < 5; i++)
        {
            CreateEmptyEntry(i + 1);
        }
    }

    private void CreateLeaderboardEntry(int rank, LeaderboardEntry entry)
    {
        if (leaderboardEntryPrefab == null)
        {
            Debug.LogError("Leaderboard Entry Prefab is NULL!");
            return;
        }

        GameObject entryObj = Instantiate(leaderboardEntryPrefab, leaderboardContainer);

        // CRITICAL: Reset local position to zero
        RectTransform rt = entryObj.GetComponent<RectTransform>();
        if (rt != null)
        {
            rt.localPosition = Vector3.zero;
            rt.localScale = Vector3.one;
            rt.anchorMin = new Vector2(0, 1);
            rt.anchorMax = new Vector2(1, 1);
            rt.pivot = new Vector2(0.5f, 1);
            rt.sizeDelta = new Vector2(0, 50); // Full width, 50 height
        }

        Debug.Log($"Created entry object: {entryObj.name} at {entryObj.transform.localPosition}");
        Debug.Log($"Entry parent: {entryObj.transform.parent.name}");
        Debug.Log($"Entry active: {entryObj.activeSelf}");
        Debug.Log($"Entry scale: {entryObj.transform.localScale}");

        TextMeshProUGUI[] texts = entryObj.GetComponentsInChildren<TextMeshProUGUI>(true);

        Debug.Log($"Found {texts.Length} text components in entry");

        foreach (var text in texts)
        {
            Debug.Log($"Text component: {text.name}, active: {text.gameObject.activeSelf}, color: {text.color}, fontSize: {text.fontSize}");
        }

        foreach (var text in texts)
        {
            text.enabled = true;
            text.color = Color.yellow;
            text.fontSize = 36;

            switch (text.name)
            {
                case "RankText":
                    text.text = rank.ToString();
                    Debug.Log($"Set RankText to '{rank}'");
                    break;

                case "NameText":
                    text.text = entry.playerName;
                    Debug.Log($"Set NameText to '{entry.playerName}'");
                    break;

                case "ScoreText":
                    text.text = entry.score.ToString();
                    Debug.Log($"Set ScoreText to '{entry.score}'");
                    break;

                case "DateText":
                    text.text = entry.date;
                    Debug.Log($"Set DateText to '{entry.date}'");
                    break;
            }
        }
    }

    private void CreateEmptyEntry(int rank)
    {
        if (leaderboardEntryPrefab == null)
        {
            Debug.LogError("Leaderboard Entry Prefab is NULL!");
            return;
        }

        GameObject entryObj = Instantiate(leaderboardEntryPrefab, leaderboardContainer);

        // CRITICAL: Reset local position to zero
        RectTransform rt = entryObj.GetComponent<RectTransform>();
        if (rt != null)
        {
            rt.localPosition = Vector3.zero;
            rt.localScale = Vector3.one;
            rt.anchorMin = new Vector2(0, 1);
            rt.anchorMax = new Vector2(1, 1);
            rt.pivot = new Vector2(0.5f, 1);
            rt.sizeDelta = new Vector2(0, 50);
        }

        TextMeshProUGUI[] texts = entryObj.GetComponentsInChildren<TextMeshProUGUI>(true);

        foreach (var text in texts)
        {
            text.enabled = true;
            text.color = Color.gray;
            text.fontSize = 36;

            switch (text.name)
            {
                case "RankText":
                    text.text = rank.ToString();
                    break;

                case "NameText":
                case "ScoreText":
                case "DateText":
                    text.text = "---";
                    break;
            }
        }
    }
}
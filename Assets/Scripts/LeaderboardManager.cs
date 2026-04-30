using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class LeaderboardEntry
{
    public string playerName;
    public int score;
    public string date;

    public LeaderboardEntry(string playerName, int score, string date)
    {
        this.playerName = playerName;
        this.score = score;
        this.date = date;
    }
}

public class LeaderboardManager : MonoBehaviour
{
    public static LeaderboardManager Instance { get; private set; }

    private const string LEADERBOARD_KEY = "Leaderboard";
    private const int MAX_ENTRIES = 5;

    private List<LeaderboardEntry> leaderboard = new List<LeaderboardEntry>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        LoadLeaderboard();
    }

    public bool IsHighScore(int score)
    {
        // Check if score is good enough for top 5
        if (leaderboard.Count < MAX_ENTRIES)
        {
            return true; // Always good if leaderboard isn't full
        }

        // Check if better than lowest score
        return score > leaderboard[leaderboard.Count - 1].score;
    }

    public void AddScore(string playerName, int score)
    {
        Debug.Log($"LeaderboardManager.AddScore called: {playerName} - {score}");

        string date = DateTime.Now.ToString("MM/dd/yyyy");
        LeaderboardEntry newEntry = new LeaderboardEntry(playerName, score, date);

        leaderboard.Add(newEntry);

        Debug.Log($"Entry added to list. Leaderboard now has {leaderboard.Count} entries");

        // Sort by score (highest first)
        leaderboard.Sort((a, b) => b.score.CompareTo(a.score));

        // Keep only top 5
        if (leaderboard.Count > MAX_ENTRIES)
        {
            leaderboard.RemoveRange(MAX_ENTRIES, leaderboard.Count - MAX_ENTRIES);
            Debug.Log($"Trimmed to {MAX_ENTRIES} entries");
        }

        SaveLeaderboard();

        Debug.Log($"Leaderboard saved with {leaderboard.Count} entries");
    }

    public List<LeaderboardEntry> GetLeaderboard()
    {
        return new List<LeaderboardEntry>(leaderboard); // Return copy
    }

    public int GetRank(int score)
    {
        for (int i = 0; i < leaderboard.Count; i++)
        {
            if (score >= leaderboard[i].score)
            {
                return i + 1; // Rank is 1-based
            }
        }

        if (leaderboard.Count < MAX_ENTRIES)
        {
            return leaderboard.Count + 1;
        }

        return -1; // Didn't make the leaderboard
    }

    private void SaveLeaderboard()
    {
        string json = JsonUtility.ToJson(new LeaderboardWrapper { entries = leaderboard });
        PlayerPrefs.SetString(LEADERBOARD_KEY, json);
        PlayerPrefs.Save();

        Debug.Log("Leaderboard saved: " + json);
    }

    private void LoadLeaderboard()
    {
        string json = PlayerPrefs.GetString(LEADERBOARD_KEY, "");

        if (!string.IsNullOrEmpty(json))
        {
            LeaderboardWrapper wrapper = JsonUtility.FromJson<LeaderboardWrapper>(json);
            leaderboard = wrapper.entries;
            Debug.Log($"Leaderboard loaded: {leaderboard.Count} entries");
        }
        else
        {
            leaderboard = new List<LeaderboardEntry>();
            Debug.Log("No leaderboard data found, starting fresh");
        }
    }

    public void ClearLeaderboard()
    {
        leaderboard.Clear();
        PlayerPrefs.DeleteKey(LEADERBOARD_KEY);
        PlayerPrefs.Save();
        Debug.Log("Leaderboard cleared");
    }

    // Wrapper class for JSON serialization (Unity can't serialize List directly)
    [System.Serializable]
    private class LeaderboardWrapper
    {
        public List<LeaderboardEntry> entries;
    }
}
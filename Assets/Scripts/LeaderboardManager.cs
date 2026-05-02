using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LeaderboardManager : MonoBehaviour
{
    public static LeaderboardManager Instance { get; private set; }

    private const string SAVE_KEY = "LeaderboardData";
    private const int MAX_ENTRIES = 5;

    private LeaderboardSaveData saveData;

    // Current session player
    public static string CurrentPlayerName { get; private set; } = "";

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        LoadData();
    }

    private void LoadData()
    {
        string json = PlayerPrefs.GetString(SAVE_KEY, "");
        if (!string.IsNullOrEmpty(json))
            saveData = JsonUtility.FromJson<LeaderboardSaveData>(json);
        else
            saveData = new LeaderboardSaveData();
    }

    private void SaveData()
    {
        string json = JsonUtility.ToJson(saveData);
        PlayerPrefs.SetString(SAVE_KEY, json);
        PlayerPrefs.Save();
    }

    public void SetCurrentPlayer(string name)
    {
        CurrentPlayerName = name;
        saveData.lastPlayerName = name;

        if (!saveData.knownPlayers.Contains(name))
            saveData.knownPlayers.Add(name);

        SaveData();
    }

    public string GetLastPlayerName() => saveData.lastPlayerName;
    public List<string> GetKnownPlayers() => saveData.knownPlayers;

    public void SubmitScore(string playerName, int score)
    {
        saveData.entries.Add(new ScoreEntry(playerName, score));

        // Sort descending and keep top 5
        saveData.entries = saveData.entries
            .OrderByDescending(e => e.score)
            .Take(MAX_ENTRIES)
            .ToList();

        SaveData();
    }

    public List<ScoreEntry> GetTopScores() => saveData.entries;
}
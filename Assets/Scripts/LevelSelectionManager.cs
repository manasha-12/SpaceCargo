using System.Collections.Generic;
using UnityEngine;

public class LevelSelectionManager : MonoBehaviour
{
    public static LevelSelectionManager Instance { get; private set; }

    // Per-player keys — player name is included so data is isolated
    private static string UnlockedKey => $"UnlockedLevels_{CurrentPlayer}";
    private static string StarsKey(int level) => $"Level{level}Stars_{CurrentPlayer}";
    private static string BestScoreKey(int level) => $"Level{level}BestScore_{CurrentPlayer}";

    private static string CurrentPlayer => LeaderboardManager.CurrentPlayerName;

    private static int maxUnlockedLevel = 1;
    private static Dictionary<int, int> levelStars = new Dictionary<int, int>();
    private static Dictionary<int, int> levelBestScores = new Dictionary<int, int>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        LoadPlayerData();
    }

    // Call this whenever the current player changes (e.g. from PlayerNameUI)
    public static void LoadPlayerData()
    {
        levelStars.Clear();
        levelBestScores.Clear();

        // Load THIS player's unlocked level count (default 1)
        maxUnlockedLevel = PlayerPrefs.GetInt(UnlockedKey, 1);

        // Load stars and scores for all their unlocked levels
        for (int i = 1; i <= maxUnlockedLevel; i++)
        {
            int stars = PlayerPrefs.GetInt(StarsKey(i), 0);
            int bestScore = PlayerPrefs.GetInt(BestScoreKey(i), 0);
            if (stars > 0) levelStars[i] = stars;
            if (bestScore > 0) levelBestScores[i] = bestScore;
        }

        Debug.Log($"LevelSelectionManager: Loaded data for '{CurrentPlayer}' — maxLevel={maxUnlockedLevel}");
    }

    public static int GetMaxUnlockedLevel() => maxUnlockedLevel;

    public static bool IsLevelUnlocked(int levelNumber) => levelNumber <= maxUnlockedLevel;

    public static void UnlockNextLevel()
    {
        maxUnlockedLevel++;
        PlayerPrefs.SetInt(UnlockedKey, maxUnlockedLevel);
        PlayerPrefs.Save();
        Debug.Log($"'{CurrentPlayer}' unlocked level {maxUnlockedLevel}!");
    }

    public static void UnlockLevel(int levelNumber)
    {
        if (levelNumber > maxUnlockedLevel)
        {
            maxUnlockedLevel = levelNumber;
            PlayerPrefs.SetInt(UnlockedKey, maxUnlockedLevel);
            PlayerPrefs.Save();
        }
    }

    public static void SetLevelStars(int levelNumber, int stars)
    {
        levelStars[levelNumber] = stars;
        PlayerPrefs.SetInt(StarsKey(levelNumber), stars);
        PlayerPrefs.Save();
        Debug.Log($"'{CurrentPlayer}' — Level {levelNumber} completed with {stars} stars!");
    }

    public static int GetLevelStars(int levelNumber)
    {
        if (levelStars.ContainsKey(levelNumber))
            return levelStars[levelNumber];
        return PlayerPrefs.GetInt(StarsKey(levelNumber), 0);
    }

    public static void SetLevelBestScore(int levelNumber, int score)
    {
        int currentBest = GetLevelBestScore(levelNumber);
        if (score > currentBest)
        {
            levelBestScores[levelNumber] = score;
            PlayerPrefs.SetInt(BestScoreKey(levelNumber), score);
            PlayerPrefs.Save();
            Debug.Log($"'{CurrentPlayer}' — New best score for Level {levelNumber}: {score}");
        }
    }

    public static int GetLevelBestScore(int levelNumber)
    {
        if (levelBestScores.ContainsKey(levelNumber))
            return levelBestScores[levelNumber];
        return PlayerPrefs.GetInt(BestScoreKey(levelNumber), 0);
    }

    public static void ResetProgress()
    {
        maxUnlockedLevel = 1;
        levelStars.Clear();
        levelBestScores.Clear();

        PlayerPrefs.SetInt(UnlockedKey, 1);
        for (int i = 1; i <= 100; i++)
        {
            PlayerPrefs.DeleteKey(StarsKey(i));
            PlayerPrefs.DeleteKey(BestScoreKey(i));
        }
        PlayerPrefs.Save();
        Debug.Log($"'{CurrentPlayer}' progress reset!");
    }
}
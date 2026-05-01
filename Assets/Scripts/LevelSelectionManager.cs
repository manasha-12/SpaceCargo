using System.Collections.Generic;
using UnityEngine;

public class LevelSelectionManager : MonoBehaviour
{
    public static LevelSelectionManager Instance { get; private set; }

    private const string UNLOCKED_LEVELS_KEY = "UnlockedLevels";
    private static int maxUnlockedLevel = 1;

    // Store level completion data
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

        // Load unlocked levels
        maxUnlockedLevel = PlayerPrefs.GetInt(UNLOCKED_LEVELS_KEY, 1);

        // Load all saved stars and scores
        LoadAllLevelData();
    }

    private void LoadAllLevelData()
    {
        // Load stars and scores for all potentially unlocked levels
        for (int i = 1; i <= maxUnlockedLevel; i++)
        {
            int stars = PlayerPrefs.GetInt($"Level{i}Stars", 0);
            int bestScore = PlayerPrefs.GetInt($"Level{i}BestScore", 0);

            if (stars > 0) levelStars[i] = stars;
            if (bestScore > 0) levelBestScores[i] = bestScore;
        }
    }

    public static int GetMaxUnlockedLevel()
    {
        return maxUnlockedLevel;
    }

    public static bool IsLevelUnlocked(int levelNumber)
    {
        return levelNumber <= maxUnlockedLevel;
    }

    public static void UnlockNextLevel()
    {
        maxUnlockedLevel++;
        PlayerPrefs.SetInt(UNLOCKED_LEVELS_KEY, maxUnlockedLevel);
        PlayerPrefs.Save();

        Debug.Log($"Unlocked level {maxUnlockedLevel}!");
    }

    public static void UnlockLevel(int levelNumber)
    {
        if (levelNumber > maxUnlockedLevel)
        {
            maxUnlockedLevel = levelNumber;
            PlayerPrefs.SetInt(UNLOCKED_LEVELS_KEY, maxUnlockedLevel);
            PlayerPrefs.Save();
        }
    }

    public static void SetLevelStars(int levelNumber, int stars)
    {
        levelStars[levelNumber] = stars;
        PlayerPrefs.SetInt($"Level{levelNumber}Stars", stars);
        PlayerPrefs.Save();

        Debug.Log($"Level {levelNumber} completed with {stars} stars!");
    }

    public static int GetLevelStars(int levelNumber)
    {
        if (levelStars.ContainsKey(levelNumber))
        {
            return levelStars[levelNumber];
        }
        return PlayerPrefs.GetInt($"Level{levelNumber}Stars", 0);
    }

    public static void SetLevelBestScore(int levelNumber, int score)
    {
        int currentBest = GetLevelBestScore(levelNumber);

        if (score > currentBest)
        {
            levelBestScores[levelNumber] = score;
            PlayerPrefs.SetInt($"Level{levelNumber}BestScore", score);
            PlayerPrefs.Save();

            Debug.Log($"New best score for Level {levelNumber}: {score}");
        }
    }

    public static int GetLevelBestScore(int levelNumber)
    {
        if (levelBestScores.ContainsKey(levelNumber))
        {
            return levelBestScores[levelNumber];
        }
        return PlayerPrefs.GetInt($"Level{levelNumber}BestScore", 0);
    }

    public static void ResetProgress()
    {
        maxUnlockedLevel = 1;
        levelStars.Clear();
        levelBestScores.Clear();

        PlayerPrefs.SetInt(UNLOCKED_LEVELS_KEY, 1);

        // Clear all level data
        for (int i = 1; i <= 100; i++) // Clear up to 100 levels
        {
            PlayerPrefs.DeleteKey($"Level{i}Stars");
            PlayerPrefs.DeleteKey($"Level{i}BestScore");
        }

        PlayerPrefs.Save();
        Debug.Log("Level selection progress reset!");
    }
}
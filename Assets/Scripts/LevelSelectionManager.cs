using UnityEngine;

public class LevelSelectionManager : MonoBehaviour
{
    public static LevelSelectionManager Instance { get; private set; }

    private const string UNLOCKED_LEVELS_KEY = "UnlockedLevels";
    private static int maxUnlockedLevel = 1; // Start with only Level 1 unlocked

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Load unlocked levels from PlayerPrefs
        maxUnlockedLevel = PlayerPrefs.GetInt(UNLOCKED_LEVELS_KEY, 1);
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

    public static void ResetProgress()
    {
        maxUnlockedLevel = 1;
        PlayerPrefs.SetInt(UNLOCKED_LEVELS_KEY, 1);
        PlayerPrefs.Save();
    }
}
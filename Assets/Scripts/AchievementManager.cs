using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AchievementManager : MonoBehaviour
{
    public static AchievementManager Instance { get; private set; }

    private const string SAVE_KEY = "AchievementData";
    private AchievementSaveData saveData;

    // Runtime tracking
    private int coinsCollectedThisLevel = 0;
    private int dronesKilledThisLevel = 0;
    private bool landedOnFirstPad = false;
    private bool pickedUpFuelThisLevel = false;
    private float levelStartTime = 0f;
    private int totalCoinsInLevel = 0;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        LoadData();
    }

    private void LoadData()
    {
        string json = PlayerPrefs.GetString(SAVE_KEY, "");
        saveData = !string.IsNullOrEmpty(json)
            ? JsonUtility.FromJson<AchievementSaveData>(json)
            : new AchievementSaveData();

        if (saveData.playerStars == null)
            saveData.playerStars = new List<PlayerStarEntry>();
    }

    private void SaveData()
    {
        PlayerPrefs.SetString(SAVE_KEY, JsonUtility.ToJson(saveData));
        PlayerPrefs.Save();
    }

    // ── Per-player helpers ────────────────────────────────────────────────
    private string CurrentPlayer => LeaderboardManager.CurrentPlayerName;

    public int GetTotalStars()
    {
        if (string.IsNullOrEmpty(CurrentPlayer)) return 0;
        var entry = saveData.playerStars
            .FirstOrDefault(p => p.playerName == CurrentPlayer);
        return entry?.stars ?? 0;
    }

    private void AddStarToCurrentPlayer(int count = 1)
    {
        if (string.IsNullOrEmpty(CurrentPlayer)) return;
        var entry = saveData.playerStars
            .FirstOrDefault(p => p.playerName == CurrentPlayer);
        if (entry == null)
        {
            entry = new PlayerStarEntry { playerName = CurrentPlayer, stars = 0 };
            saveData.playerStars.Add(entry);
        }
        entry.stars += count;
        Debug.Log($"AchievementManager: {CurrentPlayer} now has {entry.stars} stars");
    }

    // ── Per-player achievement completion ─────────────────────────────────
    // Key includes player name so each player tracks their own completions
    private string AchKey(int level, string id)
        => $"Ach_{CurrentPlayer}_{level}_{id}";

    private bool IsAchievementCompleted(int level, string id)
    {
        if (string.IsNullOrEmpty(CurrentPlayer)) return false;
        return PlayerPrefs.GetInt(AchKey(level, id), 0) == 1;
    }

    private void MarkAchievementCompleted(int level, string id)
    {
        if (string.IsNullOrEmpty(CurrentPlayer)) return;
        PlayerPrefs.SetInt(AchKey(level, id), 1);
        PlayerPrefs.Save();
    }

    // ── Level coin count ──────────────────────────────────────────────────
    public void SetTotalCoinsInLevel(int count) => totalCoinsInLevel = count;

    // ── Generate or retrieve achievements for a level ─────────────────────
    public List<Achievement> GetOrGenerateAchievements(int levelNumber)
    {
        var existing = saveData.levelAchievements
            .FirstOrDefault(l => l.levelNumber == levelNumber);

        if (existing != null)
        {
            // Refresh completion from per-player PlayerPrefs key
            foreach (var a in existing.achievements)
                a.isCompleted = IsAchievementCompleted(levelNumber, a.id);
            return existing.achievements;
        }

        var generated = GenerateForLevel(levelNumber);
        saveData.levelAchievements.Add(new LevelAchievements
        {
            levelNumber = levelNumber,
            achievements = generated
        });
        SaveData();
        return generated;
    }

    private List<Achievement> GenerateForLevel(int levelNumber)
    {
        var list = new List<Achievement>();

        if (levelNumber == 1)
        {
            list.Add(Make(levelNumber, "coins_1", "Coin Collector", "Pick up 2 coins", AchievementType.PickupCoins, 2));
            list.Add(Make(levelNumber, "first_pad_1", "Soft Touch", "Land on the first landing pad", AchievementType.LandOnFirstPad, 1));
            list.Add(Make(levelNumber, "quick_1", "Speed Runner", "Land within 30 seconds", AchievementType.LandWithinTime, 30));
        }
        else if (levelNumber == 2)
        {
            list.Add(Make(levelNumber, "coins_2", "Gold Rush", "Pick up 3 coins", AchievementType.PickupCoins, 3));
            list.Add(Make(levelNumber, "speed_2", "Feather Landing", "Land with speed below 5", AchievementType.LandWithLowSpeed, 5));
            list.Add(Make(levelNumber, "fuel_2", "Fuel Saver", "Pick up a fuel canister", AchievementType.PickupFuel, 1));
        }
        else if (levelNumber == 3)
        {
            list.Add(Make(levelNumber, "all_coins_3", "Greedy Pilot", "Collect all coins in the level", AchievementType.CollectAllCoins, 0));
            list.Add(Make(levelNumber, "health_3", "Untouchable", "Land with full health", AchievementType.LandWithFullHealth, 1));
            list.Add(Make(levelNumber, "score_3", "High Flyer", "Score over 2000 points", AchievementType.LandWithHighScore, 2000));
        }
        else
        {
            int diff = (levelNumber - 4) / 3;
            int coinTarget = Mathf.Min(3 + diff, 8);
            int droneTgt = Mathf.Min(1 + diff / 2, 3);
            int timeTgt = Mathf.Max(60 - diff * 5, 20);
            list.Add(Make(levelNumber, $"coins_{levelNumber}", "Coin Hunter", $"Pick up {coinTarget} coins", AchievementType.PickupCoins, coinTarget));
            list.Add(Make(levelNumber, $"drone_{levelNumber}", "Drone Slayer", $"Destroy {droneTgt} drone(s)", AchievementType.KillDrone, droneTgt));
            list.Add(Make(levelNumber, $"time_{levelNumber}", "Quick Lander", $"Land within {timeTgt} seconds", AchievementType.LandWithinTime, timeTgt));
        }

        return list;
    }

    private Achievement Make(int level, string id, string title,
        string desc, AchievementType type, int target)
    {
        return new Achievement
        {
            id = id,
            title = title,
            description = desc,
            type = type,
            targetValue = target,
            levelNumber = level,
            isCompleted = IsAchievementCompleted(level, id),
            starsAwarded = 1
        };
    }

    // ── Runtime event hooks ───────────────────────────────────────────────
    public void ResetLevelTracking()
    {
        coinsCollectedThisLevel = 0;
        dronesKilledThisLevel = 0;
        landedOnFirstPad = false;
        pickedUpFuelThisLevel = false;
        levelStartTime = Time.time;
        totalCoinsInLevel = 0;
    }

    public void OnCoinCollected() => coinsCollectedThisLevel++;
    public void OnDroneKilled() => dronesKilledThisLevel++;
    public void OnFuelPickedUp() => pickedUpFuelThisLevel = true;
    public void OnLandedOnFirstPad() => landedOnFirstPad = true;

    // ── Evaluate after landing ────────────────────────────────────────────
    public List<Achievement> EvaluateAchievements(int levelNumber,
        float landingSpeed, int score, bool fullHealth)
    {
        var achievements = GetOrGenerateAchievements(levelNumber);
        var newlyCompleted = new List<Achievement>();
        float elapsed = Time.time - levelStartTime;

        Debug.Log($"AchievementManager: Evaluating for player '{CurrentPlayer}', level {levelNumber}");

        foreach (var a in achievements)
        {
            if (a.isCompleted)
            {
                Debug.Log($"  [{a.id}] already completed — skipping");
                continue;
            }

            bool completed = false;
            switch (a.type)
            {
                case AchievementType.PickupCoins: completed = coinsCollectedThisLevel >= a.targetValue; break;
                case AchievementType.LandOnFirstPad: completed = landedOnFirstPad; break;
                case AchievementType.LandWithinTime: completed = elapsed <= a.targetValue; break;
                case AchievementType.LandWithLowSpeed: completed = landingSpeed <= a.targetValue; break;
                case AchievementType.CollectAllCoins: completed = totalCoinsInLevel > 0 && coinsCollectedThisLevel >= totalCoinsInLevel; break;
                case AchievementType.KillDrone: completed = dronesKilledThisLevel >= a.targetValue; break;
                case AchievementType.PickupFuel: completed = pickedUpFuelThisLevel; break;
                case AchievementType.LandWithFullHealth: completed = fullHealth; break;
                case AchievementType.LandWithHighScore: completed = score >= a.targetValue; break;
            }

            Debug.Log($"  [{a.id}] completed={completed}");

            if (completed)
            {
                a.isCompleted = true;
                AddStarToCurrentPlayer(a.starsAwarded);
                newlyCompleted.Add(a);
                MarkAchievementCompleted(levelNumber, a.id);
            }
        }

        SaveData();
        return newlyCompleted;
    }

    public List<Achievement> GetAchievementsForLevel(int levelNumber)
        => GetOrGenerateAchievements(levelNumber);

    public void ResetAllAchievements()
    {
        saveData = new AchievementSaveData();
        SaveData();
    }
}
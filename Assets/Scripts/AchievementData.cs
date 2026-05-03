using System;
using System.Collections.Generic;

[Serializable]
public enum AchievementType
{
    PickupCoins,
    LandOnFirstPad,
    LandWithinTime,
    LandWithLowSpeed,
    CollectAllCoins,
    KillDrone,
    PickupFuel,
    LandWithFullHealth,
    LandWithHighScore
}

[Serializable]
public class Achievement
{
    public string id;
    public string title;
    public string description;
    public AchievementType type;
    public int targetValue;
    public int levelNumber;
    public bool isCompleted;
    public int starsAwarded = 1;
}

[Serializable]
public class LevelAchievements
{
    public int levelNumber;
    public List<Achievement> achievements = new List<Achievement>();
}

// Stores stars per individual player by name
[Serializable]
public class PlayerStarEntry
{
    public string playerName;
    public int stars;
}

[Serializable]
public class AchievementSaveData
{
    public List<LevelAchievements> levelAchievements = new List<LevelAchievements>();
    public List<PlayerStarEntry> playerStars = new List<PlayerStarEntry>();

    // Legacy field kept for JSON migration — no longer written to
    public int totalStars;
}
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

[Serializable]
public class AchievementSaveData
{
    public List<LevelAchievements> levelAchievements = new List<LevelAchievements>();
    public int totalStars;
}
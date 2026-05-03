using UnityEngine;

/// <summary>
/// Simple static broadcaster so any enemy/hazard can know when
/// the game is waiting to start vs actively playing vs over.
/// No MonoBehaviour needed — just check the static property.
/// </summary>
public static class GameStateManager
{
    public static bool IsGameActive { get; private set; } = false;

    // Call this when lander enters Normal state
    public static void SetGameActive()
    {
        IsGameActive = true;
    }

    // Call this when lander enters WaitingToStart or GameOver
    public static void SetGameInactive()
    {
        IsGameActive = false;
    }
}
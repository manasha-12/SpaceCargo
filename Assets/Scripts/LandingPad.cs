using UnityEngine;

public class LandingPad : MonoBehaviour
{
    [SerializeField] private int scoreMultiplier;
    [SerializeField] private bool isFirstPad = false; // tick this in Inspector on pad #1

    public int GetScoreMultiplier()
    {
        return scoreMultiplier;
    }

    // Call this from Lander.cs when it detects landing on this pad
    public void OnLanderLanded()
    {
        if (isFirstPad)
        {
            AchievementManager.Instance?.OnLandedOnFirstPad();
        }
    }
}
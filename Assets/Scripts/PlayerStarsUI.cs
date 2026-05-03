using TMPro;
using UnityEngine;

public class PlayerStarsUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI starsText;
    [SerializeField] private TextMeshProUGUI playerNameText;

    // Called by Unity when object becomes active and on each scene load
    private void OnEnable() => UpdateDisplay();
    private void Start() => UpdateDisplay();

    public void UpdateDisplay()
    {
        string playerName = LeaderboardManager.CurrentPlayerName;

        // Show player name
        if (playerNameText != null)
        {
            playerNameText.text = !string.IsNullOrEmpty(playerName)
                ? $"- {playerName}"
                : "- PILOT";
        }

        // Show THIS player's stars only
        if (starsText != null)
        {
            int stars = AchievementManager.Instance != null
                ? AchievementManager.Instance.GetTotalStars()
                : 0;
            starsText.text = $"{stars}";
        }
    }
}
using TMPro;
using UnityEngine;

public class PlayerStarsUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI starsText;
    [SerializeField] private TextMeshProUGUI playerNameText;

    private void OnEnable() => UpdateDisplay();
    private void Start() => UpdateDisplay();

    public void UpdateDisplay()
    {
        if (starsText != null && AchievementManager.Instance != null)
            starsText.text = $"{AchievementManager.Instance.GetTotalStars()}";

        if (playerNameText != null &&
            !string.IsNullOrEmpty(LeaderboardManager.CurrentPlayerName))
            playerNameText.text = $"- {LeaderboardManager.CurrentPlayerName}";
    }
}
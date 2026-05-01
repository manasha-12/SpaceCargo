using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameOverUI : MonoBehaviour
{
    [SerializeField] private Button mainMenuButton;
    [SerializeField] private TextMeshProUGUI scoreTextMesh;
    [SerializeField] private TextMeshProUGUI highScoreTextMesh;
    [SerializeField] private GameObject newHighScoreLabel;

    [SerializeField] private LeaderboardUI leaderboardUI;

    private void Awake()
    {
        mainMenuButton.onClick.AddListener(() =>
        {
            SceneLoader.LoadScene(SceneLoader.Scene.MainMenuScene);
        });
    }

    private void Start()
    {
        Debug.Log("GameOverUI: Start() called");

        if (GameManager.Instance == null)
        {
            Debug.LogWarning("GameOverUI: GameManager.Instance is NULL!");

            if (scoreTextMesh != null)
                scoreTextMesh.text = "FINAL SCORE: 0";
            if (highScoreTextMesh != null)
                highScoreTextMesh.text = "HIGH SCORE: " + PlayerPrefs.GetInt("HighScore", 0);

            if (leaderboardUI != null)
            {
                leaderboardUI.DisplayLeaderboard();
            }
            return;
        }

        Debug.Log("GameOverUI: GameManager found!");

        GameManager.Instance.CheckAndSaveHighScore();

        int finalScore = GameManager.Instance.GetTotalScore();
        int highScore = GameManager.Instance.GetHighScore();
        bool isNewHighScore = GameManager.Instance.IsNewHighScore();

        Debug.Log($"GameOverUI: Final Score = {finalScore}, High Score = {highScore}");

        if (scoreTextMesh != null)
        {
            scoreTextMesh.text = "FINAL SCORE: " + finalScore.ToString();
        }

        if (highScoreTextMesh != null)
        {
            highScoreTextMesh.text = "HIGH SCORE: " + highScore.ToString();
        }

        if (newHighScoreLabel != null)
        {
            newHighScoreLabel.SetActive(isNewHighScore);
        }
    }
}
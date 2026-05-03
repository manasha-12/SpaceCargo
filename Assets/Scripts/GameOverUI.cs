using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GameOverUI : MonoBehaviour
{
    [SerializeField] private Button mainMenuButton;
    [SerializeField] private Button flyAgainButton;
    [SerializeField] private TextMeshProUGUI scoreTextMesh;
    [SerializeField] private TextMeshProUGUI highScoreTextMesh;
    [SerializeField] private GameObject newHighScoreLabel;
    [SerializeField] private LeaderboardUI leaderboardUI;

    private bool isTransitioning = false;

    private void Awake()
    {
        // Disable submit immediately on scene arrival — prevents held Cross firing on buttons
        if (GameInput.Instance != null)
            GameInput.Instance.DisableSubmitAction();

        mainMenuButton.onClick.AddListener(() =>
        {
            if (isTransitioning) return;
            isTransitioning = true;

            if (EventSystem.current != null)
                EventSystem.current.SetSelectedGameObject(null);
            if (GameInput.Instance != null)
                GameInput.Instance.DisableSubmitAction();

            StartCoroutine(LoadMainMenuAfterDelay());
        });

        if (flyAgainButton != null)
        {
            flyAgainButton.onClick.AddListener(() =>
            {
                if (isTransitioning) return;
                isTransitioning = true;

                if (EventSystem.current != null)
                    EventSystem.current.SetSelectedGameObject(null);
                if (GameInput.Instance != null)
                    GameInput.Instance.DisableSubmitAction();

                StartCoroutine(LoadGameAfterDelay());
            });
        }
    }

    private IEnumerator LoadMainMenuAfterDelay()
    {
        // 0.4s — longer than editor to account for build input latency
        yield return new WaitForSecondsRealtime(0.4f);
        SceneLoader.LoadScene(SceneLoader.Scene.MainMenuScene);
    }

    private IEnumerator LoadGameAfterDelay()
    {
        yield return new WaitForSecondsRealtime(0.4f);
        GameManager.ResetStaticData();
        SceneLoader.LoadScene(SceneLoader.Scene.GameScene);
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
                leaderboardUI.Show();

            StartCoroutine(SelectDefaultButton());
            return;
        }

        Debug.Log("GameOverUI: GameManager found!");

        GameManager.Instance.CheckAndSaveHighScore();

        int finalScore = GameManager.Instance.GetTotalScore();

        if (LeaderboardManager.Instance != null &&
            !string.IsNullOrEmpty(LeaderboardManager.CurrentPlayerName))
        {
            LeaderboardManager.Instance.SubmitScore(
                LeaderboardManager.CurrentPlayerName, finalScore);
        }

        int highScore = GameManager.Instance.GetHighScore();
        bool isNewHighScore = GameManager.Instance.IsNewHighScore();

        Debug.Log($"GameOverUI: Final Score = {finalScore}, High Score = {highScore}");

        if (scoreTextMesh != null)
            scoreTextMesh.text = "FINAL SCORE: " + finalScore.ToString();
        if (highScoreTextMesh != null)
            highScoreTextMesh.text = "HIGH SCORE: " + highScore.ToString();
        if (newHighScoreLabel != null)
            newHighScoreLabel.SetActive(isNewHighScore);
        if (leaderboardUI != null)
            leaderboardUI.DisplayLeaderboard();

        StartCoroutine(SelectDefaultButton());
    }

    private IEnumerator SelectDefaultButton()
    {
        // Deselect immediately on arrival
        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(null);

        // 0.5s — enough for controller button release in both editor and builds
        yield return new WaitForSecondsRealtime(0.5f);

        if (GameInput.Instance != null)
            GameInput.Instance.EnableSubmitAction();

        Button toSelect = flyAgainButton != null ? flyAgainButton : mainMenuButton;
        if (EventSystem.current != null && toSelect != null)
            EventSystem.current.SetSelectedGameObject(toSelect.gameObject);
    }
}
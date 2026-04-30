using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameOverUI : MonoBehaviour
{
    [SerializeField] private Button mainMenuButton;
    [SerializeField] private TextMeshProUGUI scoreTextMesh;
    [SerializeField] private TextMeshProUGUI highScoreTextMesh;


    private void Awake()
    {
        mainMenuButton.onClick.AddListener(() =>
        {
            SceneLoader.LoadScene(SceneLoader.Scene.MainMenuScene);
        });
    }

    private void Start()
    {
        GameManager.Instance.CheckAndSaveHighScore();

        int finalScore = GameManager.Instance.GetTotalScore();
        int highScore = GameManager.Instance.GetHighScore();
        bool isNewHighScore = GameManager.Instance.IsNewHighScore();

        scoreTextMesh.text = "FINAL SCORE: " + finalScore.ToString();
        highScoreTextMesh.text = "HIGH SCORE: " + highScore.ToString();

}
}

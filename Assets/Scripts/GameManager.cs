using System;
using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] private CinemachineCamera cinemachineCamera;

    private GameLevel currentLoadedLevel;

    private static int highScore;
    private const string HIGH_SCORE_KEY = "HighScore";

    private static int levelNumber = 1;
    private static int totalScore;
    [SerializeField] private List<GameLevel> gameLevelList;

    public static void ResetStaticData()
    {
        levelNumber = 1;
        totalScore = 0;
    }

    public event EventHandler OnGamePaused;
    public event EventHandler OnGameUnPaused;

    private int score;
    private float time;
    private bool isTimerActive;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        // Load high score
        if (highScore == 0)
        {
            highScore = PlayerPrefs.GetInt(HIGH_SCORE_KEY, 0);
        }

        // Load level FIRST before subscribing to events
        LoadCurrentLevel();

        // Then subscribe to events
        Lander.Instance.OnCoinPickup += Lander_OnCoinPickup;
        Lander.Instance.OnLanded += Lander_OnLanded;
        Lander.Instance.OnStateChange += Lander_OnStateChange;

        GameInput.Instance.OnMenuButtonPressed += GameInput_OnMenuButtonPressed;
    }

    private void GameInput_OnMenuButtonPressed(object sender, EventArgs e)
    {
        PauseUnPauseGame();
    }

    private void LoadCurrentLevel()
    {
        if (currentLoadedLevel != null)
        {
            Destroy(currentLoadedLevel.gameObject);
        }

        GameLevel gamelevel = GetGameLevel();

        currentLoadedLevel = Instantiate(gamelevel, Vector3.zero, Quaternion.identity);
        Lander.Instance.transform.position = currentLoadedLevel.GetLanderPosition();

        StartCoroutine(ResetHealthAfterFrame());

        // Set camera to overview position
        cinemachineCamera.Target.TrackingTarget = currentLoadedLevel.GetCameraStartTargetTransform();
        CinemachineCameraZoom2D.Instance.SetTargetOrthographicSize(currentLoadedLevel.GetZoomedOutOrthographicSize());
    }

    private System.Collections.IEnumerator ResetHealthAfterFrame()
    {
        yield return null; // Wait one frame

        if (Lander.Instance != null)
        {
            Lander.Instance.ResetHealth();
        }
    }

    private GameLevel GetGameLevel()
    {
        foreach (GameLevel gamelevel in gameLevelList)
        {
            if (gamelevel.GetLevelNumber() == levelNumber)
            {
                return gamelevel;
            }
        }
        return null;
    }

    private void Lander_OnStateChange(object sender, Lander.OnStateChangedEventAgrs e)
    {
        isTimerActive = e.state == Lander.State.Normal;

        if (e.state == Lander.State.Normal)
        {
            // Zoom in when player starts
            cinemachineCamera.Target.TrackingTarget = Lander.Instance.transform;
            CinemachineCameraZoom2D.Instance.SetNormalOrthographicSize();
        }
    }

    private void Update()
    {
        if (isTimerActive)
        {
            time += Time.deltaTime;
        }
    }

    private void Lander_OnLanded(object sender, Lander.OnLandedEventArgs e)
    {
        AddScore(e.score);
    }

    private void Lander_OnCoinPickup(object sender, System.EventArgs e)
    {
        AddScore(500);
    }

    public void AddScore(int addScoreAmount)
    {
        score += addScoreAmount;
    }

    public int GetScore()
    {
        return score;
    }

    public float GetTime()
    {
        return time;
    }

    public int GetTotalScore()
    {
        return totalScore;
    }

    public void GoToNextLevel()
    {
        levelNumber++;
        totalScore += score;

        if (currentLoadedLevel != null)
        {
            Destroy(currentLoadedLevel.gameObject);
            currentLoadedLevel = null;
        }

        if (GetGameLevel() == null)
        {
            SceneLoader.LoadScene(SceneLoader.Scene.GameOverScene);
        }
        else
        {
            SceneLoader.LoadScene(SceneLoader.Scene.GameScene);
        }
    }

    public void RetryLevel()
    {
        levelNumber = 1;
        score = 0;
        time = 0;

        if (currentLoadedLevel != null)
        {
            Destroy(currentLoadedLevel.gameObject);
            currentLoadedLevel = null;
        }

        SceneLoader.LoadScene(SceneLoader.Scene.GameScene);
    }

    public int GetLevelNumber()
    {
        return levelNumber;
    }

    public void PauseGame()
    {
        Time.timeScale = 0f;
        OnGamePaused?.Invoke(this, EventArgs.Empty);
    }

    public void UnPauseGame()
    {
        Time.timeScale = 1f;
        OnGameUnPaused?.Invoke(this, EventArgs.Empty);
    }

    public void PauseUnPauseGame()
    {
        if (Time.timeScale == 1f)
        {
            PauseGame();
        }
        else
        {
            UnPauseGame();
        }
    }

    public void CheckAndSaveHighScore()
    {
        if (totalScore > highScore)
        {
            highScore = totalScore;
            PlayerPrefs.SetInt(HIGH_SCORE_KEY, highScore);
            PlayerPrefs.Save();
        }
    }

    public int GetHighScore()
    {
        return highScore;
    }

    public bool IsNewHighScore()
    {
        return totalScore > highScore;
    }
}
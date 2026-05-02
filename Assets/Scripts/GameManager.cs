using System;
using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.SocialPlatforms.Impl;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] private CinemachineCamera cinemachineCamera;

    [SerializeField] private InputActionAsset inputActionsAsset;

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
    private bool hasLoadedLevel = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Start()
    {
        if (highScore == 0)
        {
            highScore = PlayerPrefs.GetInt(HIGH_SCORE_KEY, 0);
        }

        // Check if starting from a specific level (from Level Selection)
        if (PlayerPrefs.HasKey("StartLevel"))
        {
            levelNumber = PlayerPrefs.GetInt("StartLevel");
            PlayerPrefs.DeleteKey("StartLevel"); // Clear after reading
            totalScore = 0; // Reset score when starting from level select
        }

        if (SceneManager.GetActiveScene().name == "GameScene")
        {
            SubscribeToEvents();
            LoadCurrentLevel();
            hasLoadedLevel = true;
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Re-assign InputActions to the UI module after every scene load
        var uiModule = FindFirstObjectByType<InputSystemUIInputModule>();
        if (uiModule != null && inputActionsAsset != null)
        {
            uiModule.actionsAsset = inputActionsAsset;

            uiModule.point = InputActionReference.Create(inputActionsAsset.FindAction("Player/Point"));
            uiModule.leftClick = InputActionReference.Create(inputActionsAsset.FindAction("Player/LeftClick"));
            uiModule.move = InputActionReference.Create(inputActionsAsset.FindAction("Player/Navigate"));
            uiModule.submit = InputActionReference.Create(inputActionsAsset.FindAction("Player/Submit"));
            uiModule.cancel = InputActionReference.Create(inputActionsAsset.FindAction("Player/Cancel"));
            uiModule.scrollWheel = InputActionReference.Create(inputActionsAsset.FindAction("Player/ScrollWheel"));
        }

        if (scene.name == "GameScene" && !hasLoadedLevel)
        {
            // Find camera reference again since it was lost
            cinemachineCamera = FindFirstObjectByType<CinemachineCamera>();

            SubscribeToEvents();
            LoadCurrentLevel();
            hasLoadedLevel = true;

            // Reset achievement tracking for new level
            if (AchievementManager.Instance != null)
            {
                AchievementManager.Instance.ResetLevelTracking();

                // Count coins in the loaded level
                if (currentLoadedLevel != null)
                {
                    int coinCount = currentLoadedLevel.GetCoinCount();
                    AchievementManager.Instance.SetTotalCoinsInLevel(coinCount);
                }
            }
        }
        else if (scene.name != "GameScene")
        {
            // Clear pause event subscriptions when leaving GameScene
            OnGamePaused = null;
            OnGameUnPaused = null;

            hasLoadedLevel = false;

            // Ensure game is unpaused
            Time.timeScale = 1f;
        }
    }

    private void SubscribeToEvents()
    {
        if (Lander.Instance != null)
        {
            Lander.Instance.OnCoinPickup -= Lander_OnCoinPickup;
            Lander.Instance.OnLanded -= Lander_OnLanded;
            Lander.Instance.OnStateChange -= Lander_OnStateChange;

            Lander.Instance.OnCoinPickup += Lander_OnCoinPickup;
            Lander.Instance.OnLanded += Lander_OnLanded;
            Lander.Instance.OnStateChange += Lander_OnStateChange;
        }

        if (GameInput.Instance != null)
        {
            GameInput.Instance.OnMenuButtonPressed -= GameInput_OnMenuButtonPressed;
            GameInput.Instance.OnMenuButtonPressed += GameInput_OnMenuButtonPressed;
        }
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

        if (gamelevel == null)
        {
            Debug.LogError($"No level found for level number {levelNumber}!");
            return;
        }

        currentLoadedLevel = Instantiate(gamelevel, Vector3.zero, Quaternion.identity);

       
        if (levelNumber >= 4)
        {
            ProceduralLevelGenerator generator = currentLoadedLevel.GetComponent<ProceduralLevelGenerator>();
            if (generator != null)
            {
                generator.GenerateLevel(levelNumber);
            }
            else
            {
                Debug.LogError("ProceduralLevelGenerator not found on level!");
            }
        }

        if (Lander.Instance != null)
        {
            Lander.Instance.transform.position = currentLoadedLevel.GetLanderPosition();
            StartCoroutine(ResetHealthAfterFrame());
        }

        if (cinemachineCamera == null)
        {
            cinemachineCamera = FindFirstObjectByType<CinemachineCamera>();
        }

        if (cinemachineCamera != null && currentLoadedLevel.GetCameraStartTargetTransform() != null)
        {
            cinemachineCamera.Target.TrackingTarget = currentLoadedLevel.GetCameraStartTargetTransform();
        }

        if (CinemachineCameraZoom2D.Instance != null)
        {
            CinemachineCameraZoom2D.Instance.SetTargetOrthographicSize(currentLoadedLevel.GetZoomedOutOrthographicSize());
        }
    }

    private System.Collections.IEnumerator ResetHealthAfterFrame()
    {
        yield return null;

        if (Lander.Instance != null)
        {
            Lander.Instance.ResetHealth();
        }
    }

    private GameLevel GetGameLevel()
    {
        // For levels 1-3, use hand-made levels
        if (levelNumber <= 3)
        {
            foreach (GameLevel gamelevel in gameLevelList)
            {
                if (gamelevel.GetLevelNumber() == levelNumber)
                {
                    return gamelevel;
                }
            }
        }

        // For level 4+, use procedural generation
        if (levelNumber >= 4)
        {
            // Find the procedural level prefab
            foreach (GameLevel gamelevel in gameLevelList)
            {
                if (gamelevel.GetLevelNumber() == 4) // ProceduralLevel has level number 4
                {
                    return gamelevel;
                }
            }
        }

        return null;
    }

    private void Lander_OnStateChange(object sender, Lander.OnStateChangedEventAgrs e)
    {
        // Show achievements when lander is in waiting state (before player presses anything)
        if (e.state == Lander.State.WaitingToStart)
        {
            if (AchievementManager.Instance != null)
            {
                // Count coins in current level for CollectAllCoins achievement
                if (currentLoadedLevel != null)
                {
                    int coinCount = currentLoadedLevel.GetCoinCount();
                    AchievementManager.Instance.SetTotalCoinsInLevel(coinCount);
                }

                // Reset tracking for new level session
                AchievementManager.Instance.ResetLevelTracking();
            }

            // Show pre-level achievements panel
            if (AchievementUI.Instance != null)
            {
                AchievementUI.Instance.ShowPreLevelAchievements(levelNumber, () => { });
            }
        }

        // Hide achievement panel when player starts moving
        if (e.state == Lander.State.Normal)
        {
            AchievementUI.Instance?.HidePreLevel();

            if (cinemachineCamera == null)
                cinemachineCamera = FindFirstObjectByType<CinemachineCamera>();

            if (cinemachineCamera != null && Lander.Instance != null)
                cinemachineCamera.Target.TrackingTarget = Lander.Instance.transform;

            if (CinemachineCameraZoom2D.Instance != null)
                CinemachineCameraZoom2D.Instance.SetNormalOrthographicSize();
        }

        isTimerActive = e.state == Lander.State.Normal;
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
        AchievementManager.Instance?.OnCoinCollected();
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
        return totalScore + score;
    }

    public void GoToNextLevel()
    {
        // Calculate stars based on score (simple example)
        int stars = 1;
        if (score > 1000) stars = 2;
        if (score > 3000) stars = 3;

        // Save level completion data
        LevelSelectionManager.SetLevelStars(levelNumber, stars);
        LevelSelectionManager.SetLevelBestScore(levelNumber, score);

        levelNumber++;
        totalScore += score;
        score = 0;
        time = 0;
        hasLoadedLevel = false;

        LevelSelectionManager.UnlockLevel(levelNumber);

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
        score = 0;
        time = 0;
        hasLoadedLevel = false;

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
        int total = totalScore + score;
        if (total > highScore)
        {
            highScore = total;
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
        return (totalScore + score) > highScore;
    }

    public bool IsGameOver()
    {
        // Check if Lander has no health remaining
        if (Lander.Instance != null)
            return Lander.Instance.GetCurrentHealth() <= 0;
        return false;
    }

    // Retries same level without resetting to level 1
    public void RetryCurrentLevel()
    {
        score = 0;
        time = 0;
        hasLoadedLevel = false;

        if (currentLoadedLevel != null)
        {
            Destroy(currentLoadedLevel.gameObject);
            currentLoadedLevel = null;
        }

        SceneLoader.LoadScene(SceneLoader.Scene.GameScene);
    }
}


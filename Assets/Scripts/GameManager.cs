using System;
using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;

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
            highScore = PlayerPrefs.GetInt(HIGH_SCORE_KEY, 0);

        // Only handle GameScene if the game launched directly into it
        // (e.g. pressing Play in the Unity Editor on GameScene directly)
        if (SceneManager.GetActiveScene().name == "GameScene" && !hasLoadedLevel)
        {
            ReadStartLevelFromPrefs();
            SubscribeToEvents();
            LoadCurrentLevel();
            hasLoadedLevel = true;
        }
    }

    // ── KEY FIX ──────────────────────────────────────────────────────────────
    // Read the StartLevel key HERE — this runs every time GameScene loads,
    // unlike Start() which only fires once on the DontDestroyOnLoad object.
    private void ReadStartLevelFromPrefs()
    {
        if (PlayerPrefs.HasKey("StartLevel"))
        {
            levelNumber = PlayerPrefs.GetInt("StartLevel");
            PlayerPrefs.DeleteKey("StartLevel");
            Debug.Log($"GameManager: StartLevel read — loading level {levelNumber}");
        }
    }
    // ─────────────────────────────────────────────────────────────────────────

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
            // ── Read the selected level BEFORE loading it ──────────────────
            ReadStartLevelFromPrefs();

            cinemachineCamera = FindFirstObjectByType<CinemachineCamera>();

            SubscribeToEvents();
            LoadCurrentLevel();
            hasLoadedLevel = true;

            // Reset achievement tracking for the new level
            if (AchievementManager.Instance != null)
            {
                AchievementManager.Instance.ResetLevelTracking();

                if (currentLoadedLevel != null)
                    AchievementManager.Instance.SetTotalCoinsInLevel(
                        currentLoadedLevel.GetCoinCount());
            }
        }
        else if (scene.name != "GameScene")
        {
            OnGamePaused = null;
            OnGameUnPaused = null;
            hasLoadedLevel = false;
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
        => PauseUnPauseGame();

    private void LoadCurrentLevel()
    {
        if (currentLoadedLevel != null)
        {
            Destroy(currentLoadedLevel.gameObject);
            currentLoadedLevel = null;
        }

        GameLevel gamelevel = GetGameLevel();

        if (gamelevel == null)
        {
            Debug.LogError($"GameManager: No level found for level number {levelNumber}!");
            return;
        }

        currentLoadedLevel = Instantiate(gamelevel, Vector3.zero, Quaternion.identity);

        if (levelNumber >= 4)
        {
            ProceduralLevelGenerator generator =
                currentLoadedLevel.GetComponent<ProceduralLevelGenerator>();
            if (generator != null)
                generator.GenerateLevel(levelNumber);
            else
                Debug.LogError("GameManager: ProceduralLevelGenerator not found on level!");
        }

        if (Lander.Instance != null)
        {
            Lander.Instance.transform.position = currentLoadedLevel.GetLanderPosition();
            StartCoroutine(ResetHealthAfterFrame());
        }

        if (cinemachineCamera == null)
            cinemachineCamera = FindFirstObjectByType<CinemachineCamera>();

        if (cinemachineCamera != null &&
            currentLoadedLevel.GetCameraStartTargetTransform() != null)
            cinemachineCamera.Target.TrackingTarget =
                currentLoadedLevel.GetCameraStartTargetTransform();

        if (CinemachineCameraZoom2D.Instance != null)
            CinemachineCameraZoom2D.Instance.SetTargetOrthographicSize(
                currentLoadedLevel.GetZoomedOutOrthographicSize());

        Debug.Log($"GameManager: Loaded level {levelNumber}");
    }

    private System.Collections.IEnumerator ResetHealthAfterFrame()
    {
        yield return null;
        Lander.Instance?.ResetHealth();
    }

    private GameLevel GetGameLevel()
    {
        // Levels 1–3: hand-crafted
        if (levelNumber <= 3)
        {
            foreach (GameLevel gl in gameLevelList)
                if (gl.GetLevelNumber() == levelNumber) return gl;
        }

        // Level 4+: procedural — use the prefab marked as level 4
        if (levelNumber >= 4)
        {
            foreach (GameLevel gl in gameLevelList)
                if (gl.GetLevelNumber() == 4) return gl;
        }

        return null;
    }

    private void Lander_OnStateChange(object sender, Lander.OnStateChangedEventAgrs e)
    {
        if (e.state == Lander.State.WaitingToStart)
        {
            if (AchievementManager.Instance != null)
            {
                if (currentLoadedLevel != null)
                    AchievementManager.Instance.SetTotalCoinsInLevel(
                        currentLoadedLevel.GetCoinCount());
                AchievementManager.Instance.ResetLevelTracking();
            }

            AchievementUI.Instance?.ShowPreLevelAchievements(levelNumber, () => { });
        }

        if (e.state == Lander.State.Normal)
        {
            AchievementUI.Instance?.HidePreLevel();

            if (cinemachineCamera == null)
                cinemachineCamera = FindFirstObjectByType<CinemachineCamera>();

            if (cinemachineCamera != null && Lander.Instance != null)
                cinemachineCamera.Target.TrackingTarget = Lander.Instance.transform;

            CinemachineCameraZoom2D.Instance?.SetNormalOrthographicSize();
        }

        isTimerActive = e.state == Lander.State.Normal;
    }

    private void Update()
    {
        if (isTimerActive) time += Time.deltaTime;
    }

    private void Lander_OnLanded(object sender, Lander.OnLandedEventArgs e)
        => AddScore(e.score);

    private void Lander_OnCoinPickup(object sender, EventArgs e)
    {
        AddScore(500);
        AchievementManager.Instance?.OnCoinCollected();
    }

    public void AddScore(int amount) => score += amount;
    public int GetScore() => score;
    public float GetTime() => time;
    public int GetTotalScore() => totalScore + score;
    public int GetLevelNumber() => levelNumber;
    public int GetHighScore() => highScore;
    public bool IsNewHighScore() => (totalScore + score) > highScore;
    public bool IsGameOver() => Lander.Instance != null &&
                                        Lander.Instance.GetCurrentHealth() <= 0;

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

    public void GoToNextLevel()
    {
        int stars = 1;
        if (score > 1000) stars = 2;
        if (score > 3000) stars = 3;

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
            SceneLoader.LoadScene(SceneLoader.Scene.GameOverScene);
        else
            SceneLoader.LoadScene(SceneLoader.Scene.GameScene);
    }

    public void RetryLevel()
    {
        score = 0; time = 0; hasLoadedLevel = false;
        if (currentLoadedLevel != null)
        {
            Destroy(currentLoadedLevel.gameObject);
            currentLoadedLevel = null;
        }
        SceneLoader.LoadScene(SceneLoader.Scene.GameScene);
    }

    public void RetryCurrentLevel()
    {
        score = 0; time = 0; hasLoadedLevel = false;
        if (currentLoadedLevel != null)
        {
            Destroy(currentLoadedLevel.gameObject);
            currentLoadedLevel = null;
        }
        SceneLoader.LoadScene(SceneLoader.Scene.GameScene);
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
        if (Time.timeScale == 1f) PauseGame();
        else UnPauseGame();
    }
}
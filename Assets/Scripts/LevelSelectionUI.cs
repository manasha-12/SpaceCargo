using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelSelectionUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Transform levelButtonsContainer;
    [SerializeField] private GameObject levelButtonPrefab;
    [SerializeField] private Button backButton;

    [Header("Level Settings")]
    [SerializeField] private int totalHandMadeLevels = 3; // Levels 1-3 are hand-made
    [SerializeField] private int previewLockedCount = 3; // Show 3 locked preview cards

    private void Awake()
    {
        backButton.onClick.AddListener(() =>
        {
            SceneLoader.LoadScene(SceneLoader.Scene.MainMenuScene);
        });
    }

    private void Start()
    {
        GenerateLevelButtons();
    }

    private void GenerateLevelButtons()
    {
        // Clear existing buttons
        foreach (Transform child in levelButtonsContainer)
        {
            Destroy(child.gameObject);
        }

        int maxUnlocked = LevelSelectionManager.GetMaxUnlockedLevel();

        // Create buttons for unlocked levels
        for (int i = 1; i <= maxUnlocked; i++)
        {
            CreateLevelButton(i, true);
        }

        // Create preview cards for next 3 locked levels
        for (int i = 0; i < previewLockedCount; i++)
        {
            int lockedLevel = maxUnlocked + i + 1;
            CreateLevelButton(lockedLevel, false);
        }
    }

    private void CreateLevelButton(int levelNumber, bool isUnlocked)
    {
        GameObject buttonObj = Instantiate(levelButtonPrefab, levelButtonsContainer);

        // Find child components
        TextMeshProUGUI levelText = buttonObj.transform.Find("LevelText").GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI statusText = buttonObj.transform.Find("StatusText")?.GetComponent<TextMeshProUGUI>();
        Button button = buttonObj.GetComponent<Button>();
        Image buttonImage = buttonObj.GetComponent<Image>();

        // Set level number
        if (levelText != null)
        {
            if (levelNumber <= totalHandMadeLevels)
            {
                levelText.text = $"Level {levelNumber}";
            }
            else
            {
                levelText.text = $"Level {levelNumber}\n(Procedural)";
            }
        }

        if (isUnlocked)
        {
            // Unlocked level - clickable
            if (statusText != null)
            {
                statusText.text = "PLAY";
                statusText.color = Color.green;
            }

            if (buttonImage != null)
            {
                buttonImage.color = new Color(0.2f, 0.6f, 0.2f, 1f); // Green tint
            }

            button.interactable = true;

            int level = levelNumber; // Capture for lambda
            button.onClick.AddListener(() => LoadLevel(level));
        }
        else
        {
            // Locked level - preview only
            if (statusText != null)
            {
                statusText.text = "🔒 LOCKED";
                statusText.color = Color.gray;
            }

            if (buttonImage != null)
            {
                buttonImage.color = new Color(0.3f, 0.3f, 0.3f, 1f); // Dark gray
            }

            if (levelText != null)
            {
                levelText.color = Color.gray;
            }

            button.interactable = false;
        }
    }

    private void LoadLevel(int levelNumber)
    {
        Debug.Log($"Loading Level {levelNumber}");

        // Set the level number in GameManager
        GameManager.ResetStaticData();

        // Start from the selected level
        // We need to update GameManager to support starting from any level
        PlayerPrefs.SetInt("StartLevel", levelNumber);
        PlayerPrefs.Save();

        SceneLoader.LoadScene(SceneLoader.Scene.GameScene);
    }
}
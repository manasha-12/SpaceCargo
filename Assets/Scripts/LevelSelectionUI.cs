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
    [SerializeField] private int totalHandMadeLevels = 3;
    [SerializeField] private int previewLockedCount = 3;

    [Header("Colors")]
    [SerializeField] private Color handMadeColor = new Color(0.18f, 0.8f, 0.44f, 1f); // Emerald green
    [SerializeField] private Color proceduralColor = new Color(0.2f, 0.6f, 0.86f, 1f); // Blue
    [SerializeField] private Color lockedColor = new Color(0.2f, 0.29f, 0.37f, 1f); // Dark gray

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
        foreach (Transform child in levelButtonsContainer)
        {
            Destroy(child.gameObject);
        }

        int maxUnlocked = LevelSelectionManager.GetMaxUnlockedLevel();

        for (int i = 1; i <= maxUnlocked; i++)
        {
            CreateLevelButton(i, true);
        }

        for (int i = 0; i < previewLockedCount; i++)
        {
            int lockedLevel = maxUnlocked + i + 1;
            CreateLevelButton(lockedLevel, false);
        }
    }

    private void CreateLevelButton(int levelNumber, bool isUnlocked)
    {
        GameObject buttonObj = Instantiate(levelButtonPrefab, levelButtonsContainer);

        // Find all components
        Button button = buttonObj.GetComponent<Button>();
        Image buttonImage = buttonObj.GetComponent<Image>();

        TextMeshProUGUI levelText = buttonObj.transform.Find("LevelText")?.GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI statusText = buttonObj.transform.Find("StatusText")?.GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI typeBadge = buttonObj.transform.Find("TypeBadge")?.GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI bestScoreText = buttonObj.transform.Find("BestScoreText")?.GetComponent<TextMeshProUGUI>();

        Image previewImage = buttonObj.transform.Find("PreviewImage")?.GetComponent<Image>();
        Transform starsContainer = buttonObj.transform.Find("StarsContainer");

        // Setup preview image
        SetupPreviewImage(previewImage, levelNumber);

        // Set level text
        if (levelText != null)
        {
            levelText.text = $"LEVEL {levelNumber}";
            levelText.fontSize = 42;
            levelText.fontStyle = FontStyles.Bold;
        }

        // Set type badge
        if (typeBadge != null)
        {
            if (levelNumber <= totalHandMadeLevels)
            {
                typeBadge.text = "Hand-Made";
                typeBadge.color = new Color(1f, 0.8f, 0.2f); // Gold
            }
            else
            {
                typeBadge.text = "Procedural";
                typeBadge.color = new Color(0.5f, 0.8f, 1f); // Light blue
            }
        }

        if (isUnlocked)
        {
            SetupUnlockedButton(buttonObj, button, buttonImage, statusText, bestScoreText, starsContainer, levelNumber);
        }
        else
        {
            SetupLockedButton(buttonObj, button, buttonImage, levelText, statusText, typeBadge, bestScoreText, starsContainer);
        }
    }

    private void SetupPreviewImage(Image previewImage, int levelNumber)
    {
        if (previewImage == null) return;

        Sprite preview = Resources.Load<Sprite>($"LevelPreviews/Level{levelNumber}Preview");
        if (preview != null)
        {
            previewImage.sprite = preview;
            previewImage.color = Color.white;
        }
        else
        {
            previewImage.color = new Color(0.15f, 0.15f, 0.15f);
        }
    }

    private void SetupUnlockedButton(GameObject buttonObj, Button button, Image buttonImage,
        TextMeshProUGUI statusText, TextMeshProUGUI bestScoreText, Transform starsContainer, int levelNumber)
    {
        // Set button color based on type
        if (buttonImage != null)
        {
            buttonImage.color = (levelNumber <= totalHandMadeLevels) ? handMadeColor : proceduralColor;
        }

        // Status text
        if (statusText != null)
        {
            statusText.text = "▶ PLAY";
            statusText.color = Color.white;
            statusText.fontSize = 24;
            statusText.fontStyle = FontStyles.Bold;
        }

        // Best score
        if (bestScoreText != null)
        {
            int bestScore = LevelSelectionManager.GetLevelBestScore(levelNumber);
            if (bestScore > 0)
            {
                bestScoreText.text = $"Best: {bestScore}";
                bestScoreText.gameObject.SetActive(true);
            }
            else
            {
                bestScoreText.gameObject.SetActive(false);
            }
        }

        // Stars
        SetupStars(starsContainer, levelNumber);

        // Make button clickable
        button.interactable = true;

        // Add hover effect
        LevelButtonHover hover = buttonObj.GetComponent<LevelButtonHover>();
        if (hover == null)
        {
            hover = buttonObj.AddComponent<LevelButtonHover>();
        }

        // Setup button click
        int level = levelNumber;
        button.onClick.AddListener(() => LoadLevel(level));

        // Button color transitions
        ColorBlock colors = button.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color(1f, 1f, 0.9f);
        colors.pressedColor = new Color(0.9f, 0.9f, 0.9f);
        colors.disabledColor = new Color(0.5f, 0.5f, 0.5f);
        button.colors = colors;
    }

    private void SetupLockedButton(GameObject buttonObj, Button button, Image buttonImage,
        TextMeshProUGUI levelText, TextMeshProUGUI statusText, TextMeshProUGUI typeBadge,
        TextMeshProUGUI bestScoreText, Transform starsContainer)
    {
        // Locked appearance
        if (buttonImage != null)
        {
            buttonImage.color = lockedColor;
        }

        if (levelText != null)
        {
            levelText.color = new Color(0.6f, 0.6f, 0.6f);
        }

        if (statusText != null)
        {
            statusText.text = "🔒 LOCKED";
            statusText.color = new Color(0.5f, 0.5f, 0.5f);
            statusText.fontSize = 20;
        }

        if (typeBadge != null)
        {
            typeBadge.color = new Color(0.4f, 0.4f, 0.4f);
        }

        if (bestScoreText != null)
        {
            bestScoreText.gameObject.SetActive(false);
        }

        // Hide stars
        if (starsContainer != null)
        {
            starsContainer.gameObject.SetActive(false);
        }

        button.interactable = false;
    }

    private void SetupStars(Transform starsContainer, int levelNumber)
    {
        if (starsContainer == null) return;

        int stars = LevelSelectionManager.GetLevelStars(levelNumber);

        for (int i = 0; i < starsContainer.childCount && i < 3; i++)
        {
            Image starImage = starsContainer.GetChild(i).GetComponent<Image>();
            if (starImage != null)
            {
                if (i < stars)
                {
                    starImage.color = new Color(1f, 0.84f, 0f); // Gold
                }
                else
                {
                    starImage.color = new Color(0.3f, 0.3f, 0.3f); // Gray/empty
                }
            }
        }
    }

    private void LoadLevel(int levelNumber)
    {
        Debug.Log($"Loading Level {levelNumber}");

        GameManager.ResetStaticData();
        PlayerPrefs.SetInt("StartLevel", levelNumber);
        PlayerPrefs.Save();

        SceneLoader.LoadScene(SceneLoader.Scene.GameScene);
    }
}
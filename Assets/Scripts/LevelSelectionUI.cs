using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

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
        if (backButton != null)
        {
            backButton.onClick.AddListener(() =>
            {
                SceneLoader.LoadScene(SceneLoader.Scene.MainMenuScene);
            });

            // Enable navigation on back button
            Navigation nav = backButton.navigation;
            nav.mode = Navigation.Mode.Automatic;
            backButton.navigation = nav;
        }
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

        GameObject firstButton = null; // Track first button for selection

        for (int i = 1; i <= maxUnlocked; i++)
        {
            GameObject buttonObj = CreateLevelButton(i, true);

            // Remember first unlocked button
            if (i == 1)
            {
                firstButton = buttonObj;
            }
        }

        for (int i = 0; i < previewLockedCount; i++)
        {
            int lockedLevel = maxUnlocked + i + 1;
            CreateLevelButton(lockedLevel, false);
        }

        // Select first button for controller navigation
        if (firstButton != null)
        {
            StartCoroutine(SelectButtonAfterFrame(firstButton));
        }
    }

    // Select button after UI settles
    private System.Collections.IEnumerator SelectButtonAfterFrame(GameObject buttonObj)
    {
        yield return null; // Wait one frame

        Button button = buttonObj.GetComponent<Button>();
        if (button != null && EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(buttonObj);
            Debug.Log("Selected first level button for navigation");
        }
    }

    private GameObject CreateLevelButton(int levelNumber, bool isUnlocked) // Return GameObject
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

        return buttonObj; // Return the button object
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

        // Make button clickable and navigable
        if (button != null)
        {
            button.interactable = true;

            // Set navigation to Automatic
            Navigation nav = button.navigation;
            nav.mode = Navigation.Mode.Automatic;
            button.navigation = nav;

            // Setup button click
            int level = levelNumber;
            button.onClick.RemoveAllListeners(); // ✅ Clear existing listeners
            button.onClick.AddListener(() => LoadLevel(level));

            // Button color transitions (added selectedColor)
            ColorBlock colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1f, 1f, 0.9f);
            colors.pressedColor = new Color(0.9f, 0.9f, 0.9f);
            colors.selectedColor = new Color(1f, 1f, 0.8f); // For controller
            colors.disabledColor = new Color(0.5f, 0.5f, 0.5f);
            button.colors = colors;
        }

        // Add hover effect
        LevelButtonHover hover = buttonObj.GetComponent<LevelButtonHover>();
        if (hover == null)
        {
            hover = buttonObj.AddComponent<LevelButtonHover>();
        }
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
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PlayerNameUI : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject newNamePanel;
    [SerializeField] private GameObject existingPlayersPanel;

    [Header("New Name Panel")]
    [SerializeField] private TMP_InputField nameInputField;
    [SerializeField] private Button confirmNewNameButton;
    [SerializeField] private Button showExistingButton;

    [Header("Existing Players Panel")]
    [SerializeField] private Transform playerListContainer;
    [SerializeField] private GameObject playerButtonPrefab;
    [SerializeField] private Button newPlayerButton;

    [Header("Error")]
    [SerializeField] private TextMeshProUGUI errorText;

    private void Start()
    {
        // Guard every field before using it
        if (errorText != null)
            errorText.gameObject.SetActive(false);

        if (confirmNewNameButton != null)
            confirmNewNameButton.onClick.AddListener(OnConfirmNewName);

        if (newPlayerButton != null)
            newPlayerButton.onClick.AddListener(ShowNewNamePanel);

        if (showExistingButton != null)
            showExistingButton.onClick.AddListener(ShowExistingPlayersPanel);

        // Decide which panel to show
        if (LeaderboardManager.Instance != null)
        {
            List<string> knownPlayers = LeaderboardManager.Instance.GetKnownPlayers();
            if (knownPlayers != null && knownPlayers.Count > 0)
                ShowExistingPlayersPanel();
            else
                ShowNewNamePanel();
        }
        else
        {
            // LeaderboardManager not found — just show new name panel
            Debug.LogWarning("PlayerNameUI: LeaderboardManager.Instance is null — showing new name panel");
            ShowNewNamePanel();
        }
    }

    private void ShowNewNamePanel()
    {
        if (newNamePanel != null) newNamePanel.SetActive(true);
        if (existingPlayersPanel != null) existingPlayersPanel.SetActive(false);

        if (nameInputField != null)
        {
            // Pre-fill last name if available
            if (LeaderboardManager.Instance != null)
            {
                string last = LeaderboardManager.Instance.GetLastPlayerName();
                nameInputField.text = last;
            }
            nameInputField.ActivateInputField();
        }
    }

    private void ShowExistingPlayersPanel()
    {
        if (newNamePanel != null) newNamePanel.SetActive(false);
        if (existingPlayersPanel != null) existingPlayersPanel.SetActive(true);
        PopulatePlayerList();
    }

    private void PopulatePlayerList()
    {
        if (playerListContainer == null || playerButtonPrefab == null) return;
        if (LeaderboardManager.Instance == null) return;

        // Clear old buttons
        foreach (Transform child in playerListContainer)
            Destroy(child.gameObject);

        List<string> players = LeaderboardManager.Instance.GetKnownPlayers();
        if (players == null) return;

        foreach (string playerName in players)
        {
            GameObject btnObj = Instantiate(playerButtonPrefab, playerListContainer);
            TextMeshProUGUI label = btnObj.GetComponentInChildren<TextMeshProUGUI>();
            if (label != null) label.text = playerName;

            Button btn = btnObj.GetComponent<Button>();
            if (btn != null)
            {
                string name = playerName;
                btn.onClick.AddListener(() => SelectExistingPlayer(name));
            }
        }
    }

    private void SelectExistingPlayer(string playerName)
    {
        if (LeaderboardManager.Instance != null)
            LeaderboardManager.Instance.SetCurrentPlayer(playerName);

        StartCoroutine(ProceedToGame());
    }

    private void OnConfirmNewName()
    {
        if (nameInputField == null) return;

        string inputName = nameInputField.text.Trim();

        if (string.IsNullOrEmpty(inputName))
        {
            ShowError("Please enter a name!");
            return;
        }

        if (inputName.Length < 2)
        {
            ShowError("Name must be at least 2 characters!");
            return;
        }

        if (errorText != null)
            errorText.gameObject.SetActive(false);

        if (LeaderboardManager.Instance != null)
            LeaderboardManager.Instance.SetCurrentPlayer(inputName);

        StartCoroutine(ProceedToGame());
    }

    private void ShowError(string message)
    {
        if (errorText != null)
        {
            errorText.text = message;
            errorText.gameObject.SetActive(true);
        }
        else
        {
            Debug.LogWarning("PlayerNameUI: errorText is not assigned in Inspector!");
        }
    }

    private IEnumerator ProceedToGame()
    {
        // Disable input during transition
        if (GameInput.Instance != null)
            GameInput.Instance.DisableSubmitAction();

        yield return new WaitForSecondsRealtime(0.2f);
        SceneLoader.LoadScene(SceneLoader.Scene.LevelSelectionScene);
    }
}
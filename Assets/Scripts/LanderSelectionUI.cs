using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class LanderSelectionUI : MonoBehaviour
{
    [Header("Lander Buttons")]
    [SerializeField] private Button lander1Button;
    [SerializeField] private Button lander2Button;
    [SerializeField] private Button lander3Button;

    [Header("Selection Borders (Optional)")]
    [SerializeField] private GameObject lander1Border;
    [SerializeField] private GameObject lander2Border;
    [SerializeField] private GameObject lander3Border;

    [Header("Action Buttons")]
    [SerializeField] private Button playButton;
    [SerializeField] private Button backButton;

    private void Awake()
    {
        // Add button listeners
        lander1Button.onClick.AddListener(() => SelectLander(LanderSelectionManager.LanderType.Lander1));
        lander2Button.onClick.AddListener(() => SelectLander(LanderSelectionManager.LanderType.Lander2));
        lander3Button.onClick.AddListener(() => SelectLander(LanderSelectionManager.LanderType.Lander3));

        playButton.onClick.AddListener(() => LanderSelectionManager.Instance.ConfirmSelectionAndPlay());
        backButton.onClick.AddListener(() => LanderSelectionManager.Instance.BackToMainMenu());
    }

    private void Start()
    {
        // Show current selection
        UpdateSelectionVisual(LanderSelectionManager.Instance.GetSelectedLander());

        StartCoroutine(SelectDefaultButton());
    }

    private System.Collections.IEnumerator SelectDefaultButton()
    {
        yield return null;
        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(lander1Button.gameObject);
        }
    }

    private void SelectLander(LanderSelectionManager.LanderType landerType)
    {
        LanderSelectionManager.Instance.SelectLander(landerType);
        UpdateSelectionVisual(landerType);
    }

    private void UpdateSelectionVisual(LanderSelectionManager.LanderType landerType)
    {
        // Deactivate all borders
        if (lander1Border != null) lander1Border.SetActive(false);
        if (lander2Border != null) lander2Border.SetActive(false);
        if (lander3Border != null) lander3Border.SetActive(false);

        // Activate selected border
        switch (landerType)
        {
            case LanderSelectionManager.LanderType.Lander1:
                if (lander1Border != null) lander1Border.SetActive(true);
                break;
            case LanderSelectionManager.LanderType.Lander2:
                if (lander2Border != null) lander2Border.SetActive(true);
                break;
            case LanderSelectionManager.LanderType.Lander3:
                if (lander3Border != null) lander3Border.SetActive(true);
                break;
        }
    }
}
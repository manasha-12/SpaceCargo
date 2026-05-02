using UnityEngine;

public class LanderSelectionManager : MonoBehaviour
{
    public static LanderSelectionManager Instance { get; private set; }

    private const string SELECTED_LANDER_KEY = "SelectedLander";

    public enum LanderType
    {
        Lander1,  // Default/Red
        Lander2,  // Blue
        Lander3   // Green
    }

    [Header("Lander Sprites")]
    [SerializeField] private Sprite lander1Sprite;
    [SerializeField] private Sprite lander2Sprite;
    [SerializeField] private Sprite lander3Sprite;

    private LanderType currentSelectedLander;

    private void Awake()
    {
        Instance = this;
        LoadSelectedLander();
    }

    public void SelectLander(LanderType landerType)
    {
        currentSelectedLander = landerType;
        SaveSelectedLander();
        Debug.Log("Lander selected: " + landerType);
    }

    private void SaveSelectedLander()
    {
        PlayerPrefs.SetInt(SELECTED_LANDER_KEY, (int)currentSelectedLander);
        PlayerPrefs.Save();
    }

    private void LoadSelectedLander()
    {
        int savedLander = PlayerPrefs.GetInt(SELECTED_LANDER_KEY, 0);
        currentSelectedLander = (LanderType)savedLander;
    }

    public LanderType GetSelectedLander()
    {
        return currentSelectedLander;
    }

    public Sprite GetSelectedLanderSprite()
    {
        switch (currentSelectedLander)
        {
            case LanderType.Lander1:
                return lander1Sprite;
            case LanderType.Lander2:
                return lander2Sprite;
            case LanderType.Lander3:
                return lander3Sprite;
            default:
                return lander1Sprite;
        }
    }

    public void ConfirmSelectionAndPlay()
    {
        GameManager.ResetStaticData();
        SceneLoader.LoadScene(SceneLoader.Scene.LevelSelectionScene);
    }

    public void BackToMainMenu()
    {
        SceneLoader.LoadScene(SceneLoader.Scene.MainMenuScene);
    }
}
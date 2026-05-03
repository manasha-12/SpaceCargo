using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SettingsUI : MonoBehaviour
{
    public static SettingsUI Instance { get; private set; }

    [SerializeField] private Button settingsButton;
    [SerializeField] private Canvas targetCanvas;

    private GameObject settingsRoot;
    private CanvasGroup panelCG;

    private Image musicBtnImage;
    private Image sfxBtnImage;
    private TextMeshProUGUI musicBtnLabel;
    private TextMeshProUGUI sfxBtnLabel;
    private Button musicToggleBtn;
    private Button sfxToggleBtn;

    private bool isTransitioning = false;

    // Per-toggle cooldown — prevents double-fire from controller press+release
    private bool musicOnCooldown = false;
    private bool sfxOnCooldown = false;

    static readonly Color C_PANEL = new Color(0.04f, 0.08f, 0.18f, 0.97f);
    static readonly Color C_BORDER = new Color(0.25f, 0.55f, 1.00f, 0.90f);
    static readonly Color C_SHADOW = new Color(0.00f, 0.00f, 0.00f, 0.65f);
    static readonly Color C_GOLD = new Color(1.00f, 0.82f, 0.22f, 1f);
    static readonly Color C_TEXT = new Color(0.95f, 0.97f, 1.00f, 1f);
    static readonly Color C_BTN_ON = new Color(0.10f, 0.48f, 0.15f, 1f);
    static readonly Color C_BTN_OFF = new Color(0.55f, 0.10f, 0.10f, 1f);
    static readonly Color C_CLOSE = new Color(0.20f, 0.20f, 0.30f, 1f);

    private void Awake()
    {
        Instance = this;
        if (targetCanvas == null)
            targetCanvas = FindFirstObjectByType<Canvas>();
        BuildPanel();
        if (settingsButton != null)
            settingsButton.onClick.AddListener(Open);
    }

    private void Start()
    {
        HideImmediate();
        UpdateButtonVisuals();
    }

    // ═════════════════════════════════════════════════════════════════════
    //  BUILD PANEL
    // ═════════════════════════════════════════════════════════════════════
    private void BuildPanel()
    {
        var crt = targetCanvas.GetComponent<RectTransform>();
        const float PW = 400f, PH = 270f;

        settingsRoot = new GameObject("SettingsRoot");
        settingsRoot.transform.SetParent(crt, false);
        var rootRT = settingsRoot.AddComponent<RectTransform>();
        rootRT.anchorMin = rootRT.anchorMax = new Vector2(0.5f, 0.5f);
        rootRT.pivot = new Vector2(0.5f, 0.5f);
        rootRT.sizeDelta = new Vector2(PW + 20f, PH + 20f);
        rootRT.anchoredPosition = Vector2.zero;
        panelCG = settingsRoot.AddComponent<CanvasGroup>();

        MakeBG("Shadow", settingsRoot.transform,
            new Vector2(PW + 14f, PH + 14f), new Vector2(8f, -8f), C_SHADOW);
        MakeBG("Border", settingsRoot.transform,
            new Vector2(PW + 4f, PH + 4f), Vector2.zero, C_BORDER);

        var panelGO = MakeBG("Panel", settingsRoot.transform,
            new Vector2(PW, PH), Vector2.zero, C_PANEL);
        var panelRT = panelGO.GetComponent<RectTransform>();

        float iw = PW - 40f;
        float cy = 22f;

        PutText(panelRT, "Title", "SETTINGS",
            20f, cy, iw, 36f, 26f, C_GOLD, FontStyles.Bold, TextAlignmentOptions.Center);
        cy += 46f;

        var div = NewRT("Div", panelRT);
        SetRect(div, 20f, cy, iw, 1f);
        div.gameObject.AddComponent<Image>().color = new Color(0.3f, 0.6f, 1f, 0.4f);
        cy += 12f;

        // Music button
        var mRT = NewRT("MusicBtn", panelRT);
        SetRect(mRT, 20f, cy, iw, 56f);
        musicBtnImage = mRT.gameObject.AddComponent<Image>();
        musicBtnImage.color = C_BTN_ON;
        musicToggleBtn = mRT.gameObject.AddComponent<Button>();
        SetTransparentColorBlock(musicToggleBtn);
        musicBtnLabel = PutText(mRT, "Lbl", "MUSIC  ON",
            0f, 0f, iw, 56f, 20f, C_TEXT, FontStyles.Bold, TextAlignmentOptions.Center);
        musicToggleBtn.onClick.AddListener(OnMusicToggle);
        mRT.gameObject.AddComponent<Shadow>().effectColor = new Color(0, 0, 0, 0.5f);
        cy += 64f;

        // SFX button
        var sRT = NewRT("SFXBtn", panelRT);
        SetRect(sRT, 20f, cy, iw, 56f);
        sfxBtnImage = sRT.gameObject.AddComponent<Image>();
        sfxBtnImage.color = C_BTN_ON;
        sfxToggleBtn = sRT.gameObject.AddComponent<Button>();
        SetTransparentColorBlock(sfxToggleBtn);
        sfxBtnLabel = PutText(sRT, "Lbl", "SOUND EFFECTS  ON",
            0f, 0f, iw, 56f, 20f, C_TEXT, FontStyles.Bold, TextAlignmentOptions.Center);
        sfxToggleBtn.onClick.AddListener(OnSFXToggle);
        sRT.gameObject.AddComponent<Shadow>().effectColor = new Color(0, 0, 0, 0.5f);
        cy += 64f;

        // Close button
        var cRT = NewRT("CloseBtn", panelRT);
        SetRect(cRT, 20f, cy, iw, 40f);
        cRT.gameObject.AddComponent<Image>().color = C_CLOSE;
        var closeBtn = cRT.gameObject.AddComponent<Button>();
        SetTransparentColorBlock(closeBtn);
        PutText(cRT, "Lbl", "CLOSE",
            0f, 0f, iw, 40f, 16f, C_TEXT, FontStyles.Bold, TextAlignmentOptions.Center);
        closeBtn.onClick.AddListener(Close);
        cRT.gameObject.AddComponent<Shadow>().effectColor = new Color(0, 0, 0, 0.5f);

        musicToggleBtn.navigation = new Navigation
        {
            mode = Navigation.Mode.Explicit,
            selectOnUp = closeBtn,
            selectOnDown = sfxToggleBtn,
        };
        sfxToggleBtn.navigation = new Navigation
        {
            mode = Navigation.Mode.Explicit,
            selectOnUp = musicToggleBtn,
            selectOnDown = closeBtn,
        };
        closeBtn.navigation = new Navigation
        {
            mode = Navigation.Mode.Explicit,
            selectOnUp = sfxToggleBtn,
            selectOnDown = musicToggleBtn,
        };
    }

    // ═════════════════════════════════════════════════════════════════════
    //  OPEN / CLOSE
    // ═════════════════════════════════════════════════════════════════════
    public void Open()
    {
        if (isTransitioning) return;
        isTransitioning = true;

        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(null);

        Time.timeScale = 0f;
        settingsRoot.SetActive(true);
        panelCG.alpha = 1f;
        panelCG.interactable = true;
        panelCG.blocksRaycasts = true;
        UpdateButtonVisuals();

        StartCoroutine(OpenAfterDelay());
    }

    private IEnumerator OpenAfterDelay()
    {
        yield return new WaitForSecondsRealtime(0.3f);
        isTransitioning = false;
        musicOnCooldown = false;
        sfxOnCooldown = false;
        if (musicToggleBtn != null && EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(musicToggleBtn.gameObject);
    }

    public void Close()
    {
        if (isTransitioning) return;
        isTransitioning = true;

        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(null);

        Time.timeScale = 1f;
        HideImmediate();
        StartCoroutine(CloseAfterDelay());
    }

    public void CloseImmediate()
    {
        isTransitioning = false;
        HideImmediate();
    }

    private IEnumerator CloseAfterDelay()
    {
        yield return new WaitForSecondsRealtime(0.3f);
        isTransitioning = false;
        if (settingsButton != null && EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(settingsButton.gameObject);
    }

    private void HideImmediate()
    {
        if (settingsRoot != null) settingsRoot.SetActive(false);
        if (panelCG != null)
        {
            panelCG.alpha = 0f;
            panelCG.interactable = false;
            panelCG.blocksRaycasts = false;
        }
    }

    // ═════════════════════════════════════════════════════════════════════
    //  TOGGLE HANDLERS
    //  Each toggle has its own cooldown flag so a controller press+release
    //  (which fires onClick twice) only counts as one toggle.
    // ═════════════════════════════════════════════════════════════════════
    private void OnMusicToggle()
    {
        if (isTransitioning || musicOnCooldown) return;
        if (AudioManager.Instance == null) return;

        musicOnCooldown = true;
        AudioManager.Instance.ToggleMusic();
        UpdateButtonVisuals();
        StartCoroutine(ResetMusicCooldown());
    }

    private IEnumerator ResetMusicCooldown()
    {
        // 0.4s cooldown — longer than a controller press+release cycle (~0.1-0.2s)
        yield return new WaitForSecondsRealtime(0.4f);
        musicOnCooldown = false;
    }

    private void OnSFXToggle()
    {
        if (isTransitioning || sfxOnCooldown) return;
        if (AudioManager.Instance == null) return;

        sfxOnCooldown = true;
        AudioManager.Instance.ToggleSFX();
        UpdateButtonVisuals();
        StartCoroutine(ResetSFXCooldown());
    }

    private IEnumerator ResetSFXCooldown()
    {
        yield return new WaitForSecondsRealtime(0.4f);
        sfxOnCooldown = false;
    }

    // ═════════════════════════════════════════════════════════════════════
    //  VISUALS
    // ═════════════════════════════════════════════════════════════════════
    private void UpdateButtonVisuals()
    {
        if (AudioManager.Instance == null) return;

        bool music = AudioManager.Instance.MusicEnabled;
        bool sfx = AudioManager.Instance.SFXEnabled;

        if (musicBtnImage != null) musicBtnImage.color = music ? C_BTN_ON : C_BTN_OFF;
        if (sfxBtnImage != null) sfxBtnImage.color = sfx ? C_BTN_ON : C_BTN_OFF;
        if (musicBtnLabel != null) musicBtnLabel.text = music ? "MUSIC  ON" : "MUSIC  OFF";
        if (sfxBtnLabel != null) sfxBtnLabel.text = sfx ? "SOUND EFFECTS  ON" : "SOUND EFFECTS  OFF";
    }

    // ═════════════════════════════════════════════════════════════════════
    //  HELPERS
    // ═════════════════════════════════════════════════════════════════════
    private static GameObject MakeBG(string name, Transform parent, Vector2 size,
        Vector2 offset, Color color)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = size;
        rt.anchoredPosition = offset;
        go.AddComponent<Image>().color = color;
        return go;
    }

    private static RectTransform NewRT(string name, Transform parent)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        return go.AddComponent<RectTransform>();
    }

    private static void SetRect(RectTransform rt, float x, float y, float w, float h)
    {
        rt.anchorMin = new Vector2(0f, 1f);
        rt.anchorMax = new Vector2(0f, 1f);
        rt.pivot = new Vector2(0f, 1f);
        rt.sizeDelta = new Vector2(w, h);
        rt.anchoredPosition = new Vector2(x, -y);
    }

    private static TextMeshProUGUI PutText(RectTransform parent, string name, string text,
        float x, float y, float w, float h,
        float size, Color color, FontStyles style, TextAlignmentOptions align)
    {
        var rt = NewRT(name, parent);
        rt.anchorMin = new Vector2(0f, 1f);
        rt.anchorMax = new Vector2(0f, 1f);
        rt.pivot = new Vector2(0f, 1f);
        rt.sizeDelta = new Vector2(w, h);
        rt.anchoredPosition = new Vector2(x, -y);
        var tmp = rt.gameObject.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = size;
        tmp.fontStyle = style;
        tmp.color = color;
        tmp.alignment = align;
        tmp.enableWordWrapping = false;
        return tmp;
    }

    private static void SetTransparentColorBlock(Button btn)
    {
        var cb = btn.colors;
        cb.normalColor = new Color(1f, 1f, 1f, 1f);
        cb.highlightedColor = new Color(1f, 1f, 1f, 0.85f);
        cb.pressedColor = new Color(0.8f, 0.8f, 0.8f, 1f);
        cb.selectedColor = new Color(1f, 1f, 1f, 0.9f);
        cb.colorMultiplier = 1f;
        cb.fadeDuration = 0.05f;
        btn.colors = cb;
    }
}
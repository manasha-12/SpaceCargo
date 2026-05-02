using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PlayerNameUI : MonoBehaviour
{
    [Header("Assign the Canvas")]
    [SerializeField] private Canvas targetCanvas;

    private GameObject newNamePanel;
    private GameObject existingPlayersPanel;
    private TMP_InputField nameInputField;
    private Button confirmButton;
    private Button showExistingButton;
    private Button newPilotButton;
    private Button backButtonNew;
    private Button backButtonExisting;
    private Transform playerListContainer;
    private TextMeshProUGUI errorText;
    private readonly List<GameObject> rootObjects = new List<GameObject>();

    // Track all buttons per panel for navigation setup
    private readonly List<Button> newPanelButtons = new List<Button>();
    private readonly List<Button> existingPanelButtons = new List<Button>();

    // Palette
    static readonly Color C_OVERLAY = new Color(0.02f, 0.04f, 0.12f, 0.93f);
    static readonly Color C_PANEL = new Color(0.05f, 0.09f, 0.20f, 0.98f);
    static readonly Color C_BORDER = new Color(0.25f, 0.55f, 1.00f, 1.00f);
    static readonly Color C_SHADOW = new Color(0.00f, 0.00f, 0.00f, 0.60f);
    static readonly Color C_GOLD = new Color(1.00f, 0.82f, 0.22f, 1.00f);
    static readonly Color C_BLUE = new Color(0.20f, 0.55f, 1.00f, 1.00f);
    static readonly Color C_BTN = new Color(0.14f, 0.48f, 0.95f, 1.00f);
    static readonly Color C_BTN_DARK = new Color(0.08f, 0.14f, 0.28f, 1.00f);
    static readonly Color C_BTN_BACK = new Color(0.06f, 0.10f, 0.22f, 1.00f);
    static readonly Color C_INPUT = new Color(0.02f, 0.04f, 0.10f, 1.00f);
    static readonly Color C_INPUT_BD = new Color(0.30f, 0.60f, 1.00f, 0.90f);
    static readonly Color C_TEXT = new Color(0.95f, 0.97f, 1.00f, 1.00f);
    static readonly Color C_MUTED = new Color(0.58f, 0.68f, 0.82f, 1.00f);
    static readonly Color C_ERROR = new Color(1.00f, 0.32f, 0.32f, 1.00f);
    static readonly Color C_ROW = new Color(0.07f, 0.13f, 0.26f, 1.00f);

    static RectTransform UI(string name, Transform parent)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        return go.AddComponent<RectTransform>();
    }

    static void Stretch(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
    }

    static RectTransform FromTop(string name, Transform parent, float y, float w, float h)
    {
        var rt = UI(name, parent);
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 1f);
        rt.pivot = new Vector2(0.5f, 1f);
        rt.sizeDelta = new Vector2(w, h);
        rt.anchoredPosition = new Vector2(0f, -y);
        return rt;
    }

    // Set Navigation.Mode.Explicit up/down chain for a list of buttons
    static void SetupNavigation(List<Button> buttons)
    {
        for (int i = 0; i < buttons.Count; i++)
        {
            var nav = new Navigation { mode = Navigation.Mode.Explicit };
            nav.selectOnUp = i > 0 ? buttons[i - 1] : buttons[buttons.Count - 1];
            nav.selectOnDown = i < buttons.Count - 1 ? buttons[i + 1] : buttons[0];
            buttons[i].navigation = nav;
        }
    }

    // ── Lifecycle ─────────────────────────────────────────────────────────────
    private void Awake()
    {
        var crt = (targetCanvas != null ? targetCanvas
                  : FindFirstObjectByType<Canvas>()).GetComponent<RectTransform>();

        for (int i = crt.childCount - 1; i >= 0; i--)
            Destroy(crt.GetChild(i).gameObject);

        BuildBG(crt);
        BuildTitle(crt);

        newNamePanel = BuildNewNamePanel(crt);
        existingPlayersPanel = BuildExistingPanel(crt);

        rootObjects.Add(newNamePanel);
        rootObjects.Add(existingPlayersPanel);

        // Wire navigation for static buttons (player rows wired separately in PopulatePlayerList)
        SetupNavigation(newPanelButtons);

        newNamePanel.SetActive(false);
        existingPlayersPanel.SetActive(false);
    }

    private void Start()
    {
        // Clear any held input from previous scene
        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(null);

        if (GameInput.Instance != null)
            GameInput.Instance.DisableSubmitAction();

        confirmButton?.onClick.AddListener(OnConfirm);
        newPilotButton?.onClick.AddListener(ShowNewPanel);
        showExistingButton?.onClick.AddListener(ShowExistingPanel);
        errorText?.gameObject.SetActive(false);

        if (LeaderboardManager.Instance != null)
        {
            var known = LeaderboardManager.Instance.GetKnownPlayers();
            if (known != null && known.Count > 0)
                ShowExistingPanel();
            else
                ShowNewPanel();
        }
        else ShowNewPanel();
    }

    private void OnDestroy()
    {
        foreach (var go in rootObjects)
            if (go != null) Destroy(go);
        rootObjects.Clear();
    }

    // ── Background ────────────────────────────────────────────────────────────
    void BuildBG(RectTransform c)
    {
        var ov = UI("Overlay", c); ov.transform.SetAsFirstSibling();
        Stretch(ov); ov.gameObject.AddComponent<Image>().color = C_OVERLAY;
        rootObjects.Add(ov.gameObject);

        var stars = UI("Stars", c); Stretch(stars);
        rootObjects.Add(stars.gameObject);
        var rng = new System.Random(42);
        for (int i = 0; i < 55; i++)
        {
            var s = UI($"S{i}", stars);
            s.gameObject.AddComponent<Image>().color =
                new Color(0.7f, 0.8f, 1f, (float)rng.NextDouble() * 0.4f + 0.1f);
            float sz = (float)rng.NextDouble() * 3f + 1f;
            s.sizeDelta = new Vector2(sz, sz);
            s.anchorMin = s.anchorMax =
                new Vector2((float)rng.NextDouble(), (float)rng.NextDouble());
            s.anchoredPosition = Vector2.zero;
        }
    }

    void BuildTitle(RectTransform c)
    {
        var trt = UI("Title", c);
        rootObjects.Add(trt.gameObject);
        trt.anchorMin = new Vector2(0f, 1f); trt.anchorMax = new Vector2(1f, 1f);
        trt.pivot = new Vector2(0.5f, 1f);
        trt.sizeDelta = new Vector2(0f, 80f);
        trt.anchoredPosition = new Vector2(0f, -24f);
        var t = trt.gameObject.AddComponent<TextMeshProUGUI>();
        t.text = "PILOT REGISTRATION"; t.fontSize = 46f;
        t.fontStyle = FontStyles.Bold; t.color = C_GOLD;
        t.alignment = TextAlignmentOptions.Center; t.characterSpacing = 3f;

        var lrt = UI("TitleLine", c);
        rootObjects.Add(lrt.gameObject);
        lrt.anchorMin = new Vector2(0.18f, 1f); lrt.anchorMax = new Vector2(0.82f, 1f);
        lrt.pivot = new Vector2(0.5f, 1f);
        lrt.sizeDelta = new Vector2(0f, 2f);
        lrt.anchoredPosition = new Vector2(0f, -108f);
        lrt.gameObject.AddComponent<Image>().color =
            new Color(C_BLUE.r, C_BLUE.g, C_BLUE.b, 0.6f);
    }

    // ── Wrapper ───────────────────────────────────────────────────────────────
    GameObject MakeWrapper(string name, RectTransform canvas, float w, float h,
        out RectTransform contentParent)
    {
        var wrapper = UI($"{name}_Wrapper", canvas);
        wrapper.anchorMin = wrapper.anchorMax = new Vector2(0.5f, 0.5f);
        wrapper.pivot = new Vector2(0.5f, 0.5f);
        wrapper.sizeDelta = new Vector2(w + 20f, h + 20f);
        wrapper.anchoredPosition = Vector2.zero;

        var sh = UI("Shadow", wrapper);
        sh.anchorMin = sh.anchorMax = new Vector2(0.5f, 0.5f);
        sh.pivot = new Vector2(0.5f, 0.5f);
        sh.sizeDelta = new Vector2(w + 18f, h + 18f);
        sh.anchoredPosition = new Vector2(8f, -8f);
        sh.gameObject.AddComponent<Image>().color = C_SHADOW;

        var bd = UI("Border", wrapper);
        bd.anchorMin = bd.anchorMax = new Vector2(0.5f, 0.5f);
        bd.pivot = new Vector2(0.5f, 0.5f);
        bd.sizeDelta = new Vector2(w + 4f, h + 4f);
        bd.anchoredPosition = Vector2.zero;
        bd.gameObject.AddComponent<Image>().color = C_BORDER;

        var pn = UI("Panel", wrapper);
        pn.anchorMin = pn.anchorMax = new Vector2(0.5f, 0.5f);
        pn.pivot = new Vector2(0.5f, 1f);
        pn.sizeDelta = new Vector2(w, h);
        pn.anchoredPosition = new Vector2(0f, h / 2f);
        pn.gameObject.AddComponent<Image>().color = C_PANEL;

        contentParent = pn;
        return wrapper.gameObject;
    }

    // ═════════════════════════════════════════════════════════════════════════
    //  NEW NAME PANEL
    // ═════════════════════════════════════════════════════════════════════════
    GameObject BuildNewNamePanel(RectTransform canvas)
    {
        float w = 480f, h = 440f;
        RectTransform pn;
        var wrapper = MakeWrapper("NewName", canvas, w, h, out pn);

        float iw = w - 60f;
        float y = 28f;

        Txt(FromTop("Header", pn, y, iw, 38f),
            "NEW PILOT", 26f, C_GOLD, FontStyles.Bold, TextAlignmentOptions.Center);
        y += 48f;

        Line(FromTop("L1", pn, y, iw, 1f)); y += 12f;

        Txt(FromTop("Sub", pn, y, iw, 22f),
            "Enter your callsign to begin", 15f, C_MUTED,
            FontStyles.Normal, TextAlignmentOptions.Center);
        y += 32f;

        Txt(FromTop("Lbl", pn, y, iw, 18f),
            "CALLSIGN", 11f,
            new Color(C_BLUE.r, C_BLUE.g, C_BLUE.b, 0.85f),
            FontStyles.Bold, TextAlignmentOptions.Left);
        y += 22f;

        nameInputField = BuildInputField(pn, y, iw, 56f); y += 64f;

        var err = FromTop("Err", pn, y, iw, 22f);
        errorText = err.gameObject.AddComponent<TextMeshProUGUI>();
        errorText.fontSize = 14f; errorText.color = C_ERROR;
        errorText.alignment = TextAlignmentOptions.Center;
        y += 28f;

        confirmButton = Btn(FromTop("Confirm", pn, y, iw, 52f),
            "LAUNCH MISSION", C_BTN, C_TEXT, 19f, FontStyles.Bold);
        newPanelButtons.Add(confirmButton);
        y += 62f;

        Line(FromTop("L2", pn, y, iw, 1f)); y += 12f;

        showExistingButton = Btn(FromTop("Switch", pn, y, iw, 38f),
            "RETURNING PILOT? CLICK HERE",
            C_BTN_DARK, C_MUTED, 13f, FontStyles.Normal);
        newPanelButtons.Add(showExistingButton);
        y += 48f;

        backButtonNew = Btn(FromTop("BackBtn", pn, y, iw, 38f),
            "< BACK TO MAIN MENU", C_BTN_BACK, C_MUTED, 13f, FontStyles.Normal);
        backButtonNew.onClick.AddListener(GoBackToMainMenu);
        newPanelButtons.Add(backButtonNew);

        return wrapper;
    }

    // ═════════════════════════════════════════════════════════════════════════
    //  EXISTING PLAYERS PANEL
    // ═════════════════════════════════════════════════════════════════════════
    GameObject BuildExistingPanel(RectTransform canvas)
    {
        float w = 480f, h = 470f;
        RectTransform pn;
        var wrapper = MakeWrapper("Existing", canvas, w, h, out pn);

        float iw = w - 60f;
        float y = 28f;

        Txt(FromTop("Header", pn, y, iw, 38f),
            "SELECT PILOT", 26f, C_GOLD, FontStyles.Bold, TextAlignmentOptions.Center);
        y += 48f;

        Line(FromTop("L1", pn, y, iw, 1f)); y += 12f;

        Txt(FromTop("Sub", pn, y, iw, 22f),
            "Choose your callsign", 15f, C_MUTED,
            FontStyles.Normal, TextAlignmentOptions.Center);
        y += 32f;

        float sh = 210f;
        var scrollRt = FromTop("Scroll", pn, y, iw, sh);
        scrollRt.gameObject.AddComponent<Image>().color =
            new Color(C_BLUE.r, C_BLUE.g, C_BLUE.b, 0.10f);
        BuildScroll(scrollRt);
        y += sh + 14f;

        Line(FromTop("L2", pn, y, iw, 1f)); y += 12f;

        newPilotButton = Btn(FromTop("NewPilot", pn, y, iw, 40f),
            "+ NEW PILOT", C_BTN_DARK, C_MUTED, 14f, FontStyles.Normal);
        existingPanelButtons.Add(newPilotButton);
        y += 50f;

        backButtonExisting = Btn(FromTop("BackBtn", pn, y, iw, 38f),
            "< BACK TO MAIN MENU", C_BTN_BACK, C_MUTED, 13f, FontStyles.Normal);
        backButtonExisting.onClick.AddListener(GoBackToMainMenu);
        existingPanelButtons.Add(backButtonExisting);

        return wrapper;
    }

    void BuildScroll(RectTransform holder)
    {
        var scroll = holder.gameObject.AddComponent<ScrollRect>();
        scroll.horizontal = false;

        var vp = UI("VP", holder); Stretch(vp);
        vp.gameObject.AddComponent<Image>().color = new Color(0, 0, 0, 0.01f);
        var mask = vp.gameObject.AddComponent<Mask>();
        mask.showMaskGraphic = false;
        scroll.viewport = vp;

        var content = UI("Content", vp);
        content.anchorMin = new Vector2(0f, 1f);
        content.anchorMax = new Vector2(1f, 1f);
        content.pivot = new Vector2(0.5f, 1f);
        content.anchoredPosition = Vector2.zero;
        content.sizeDelta = Vector2.zero;
        var vg = content.gameObject.AddComponent<VerticalLayoutGroup>();
        vg.spacing = 6f; vg.padding = new RectOffset(4, 4, 4, 4);
        vg.childControlWidth = true; vg.childControlHeight = true;
        vg.childForceExpandWidth = true; vg.childForceExpandHeight = false;
        content.gameObject.AddComponent<ContentSizeFitter>().verticalFit =
            ContentSizeFitter.FitMode.PreferredSize;
        scroll.content = content;
        playerListContainer = content;
    }

    // ═════════════════════════════════════════════════════════════════════════
    //  ELEMENT HELPERS
    // ═════════════════════════════════════════════════════════════════════════
    static void Txt(RectTransform rt, string text, float size, Color color,
        FontStyles style, TextAlignmentOptions align)
    {
        var t = rt.gameObject.AddComponent<TextMeshProUGUI>();
        t.text = text; t.fontSize = size; t.fontStyle = style;
        t.color = color; t.alignment = align;
    }

    static void Line(RectTransform rt)
    {
        rt.gameObject.AddComponent<Image>().color =
            new Color(C_BLUE.r, C_BLUE.g, C_BLUE.b, 0.30f);
    }

    static Button Btn(RectTransform rt, string label, Color bg, Color textColor,
        float size, FontStyles style)
    {
        rt.gameObject.AddComponent<Image>().color = bg;
        var btn = rt.gameObject.AddComponent<Button>();
        var cb = btn.colors;
        cb.normalColor = Color.white;
        cb.highlightedColor = new Color(1.3f, 1.3f, 1.3f, 1f);
        cb.pressedColor = new Color(0.70f, 0.70f, 0.70f, 1f);
        cb.selectedColor = new Color(1.15f, 1.15f, 1f, 1f);
        cb.colorMultiplier = 1f; cb.fadeDuration = 0.08f;
        btn.colors = cb;
        rt.gameObject.AddComponent<Shadow>().effectColor = new Color(0, 0, 0, 0.5f);

        var lrt = UI("Label", rt);
        lrt.anchorMin = Vector2.zero; lrt.anchorMax = Vector2.one;
        lrt.offsetMin = new Vector2(6f, 0f); lrt.offsetMax = new Vector2(-6f, 0f);
        var tmp = lrt.gameObject.AddComponent<TextMeshProUGUI>();
        tmp.text = label; tmp.fontSize = size; tmp.fontStyle = style;
        tmp.color = textColor; tmp.alignment = TextAlignmentOptions.Center;
        tmp.overflowMode = TextOverflowModes.Ellipsis;
        return btn;
    }

    TMP_InputField BuildInputField(RectTransform parent, float y, float w, float h)
    {
        var bg = FromTop("InputBG", parent, y, w, h);
        bg.gameObject.AddComponent<Image>().color = C_INPUT;
        var ol = bg.gameObject.AddComponent<Outline>();
        ol.effectColor = C_INPUT_BD; ol.effectDistance = new Vector2(1.5f, -1.5f);

        var ifRt = UI("IF", bg);
        ifRt.anchorMin = Vector2.zero; ifRt.anchorMax = Vector2.one;
        ifRt.offsetMin = new Vector2(12f, 4f); ifRt.offsetMax = new Vector2(-12f, -4f);
        var field = ifRt.gameObject.AddComponent<TMP_InputField>();

        var ta = UI("TA", ifRt);
        ta.anchorMin = Vector2.zero; ta.anchorMax = Vector2.one;
        ta.offsetMin = ta.offsetMax = Vector2.zero;
        ta.gameObject.AddComponent<RectMask2D>();

        var ph = UI("PH", ta);
        ph.anchorMin = Vector2.zero; ph.anchorMax = Vector2.one;
        ph.offsetMin = ph.offsetMax = Vector2.zero;
        var phT = ph.gameObject.AddComponent<TextMeshProUGUI>();
        phT.text = "e.g. AceFlyer99"; phT.fontSize = 19f;
        phT.fontStyle = FontStyles.Italic;
        phT.color = new Color(0.35f, 0.45f, 0.60f, 1f);
        phT.alignment = TextAlignmentOptions.MidlineLeft;

        var tx = UI("TX", ta);
        tx.anchorMin = Vector2.zero; tx.anchorMax = Vector2.one;
        tx.offsetMin = tx.offsetMax = Vector2.zero;
        var txT = tx.gameObject.AddComponent<TextMeshProUGUI>();
        txT.fontSize = 22f; txT.color = C_TEXT;
        txT.alignment = TextAlignmentOptions.MidlineLeft;

        field.textViewport = ta; field.textComponent = txT;
        field.placeholder = phT; field.characterLimit = 20;
        field.contentType = TMP_InputField.ContentType.Standard;
        return field;
    }

    // ═════════════════════════════════════════════════════════════════════════
    //  PLAYER LIST
    // ═════════════════════════════════════════════════════════════════════════
    void PopulatePlayerList()
    {
        if (playerListContainer == null || LeaderboardManager.Instance == null) return;
        foreach (Transform c in playerListContainer) Destroy(c.gameObject);

        var players = LeaderboardManager.Instance.GetKnownPlayers();
        if (players == null || players.Count == 0) return;

        var rowButtons = new List<Button>();

        foreach (string pName in players)
        {
            var row = UI($"Row_{pName}", playerListContainer);
            row.gameObject.AddComponent<Image>().color = C_ROW;
            var btn = row.gameObject.AddComponent<Button>();
            var cb = btn.colors;
            cb.normalColor = Color.white;
            cb.highlightedColor = new Color(1.25f, 1.25f, 1.25f, 1f);
            cb.pressedColor = new Color(0.72f, 0.72f, 0.72f, 1f);
            cb.selectedColor = new Color(1.15f, 1.15f, 1f, 1f);
            cb.colorMultiplier = 1f; btn.colors = cb;

            var le = row.gameObject.AddComponent<LayoutElement>();
            le.preferredHeight = 52f; le.minHeight = 52f; le.flexibleWidth = 1f;

            row.gameObject.AddComponent<Shadow>().effectColor = new Color(0, 0, 0, 0.4f);

            var hg = row.gameObject.AddComponent<HorizontalLayoutGroup>();
            hg.childAlignment = TextAnchor.MiddleLeft; hg.spacing = 10f;
            hg.padding = new RectOffset(14, 14, 0, 0);
            hg.childControlHeight = true; hg.childForceExpandHeight = true;
            hg.childControlWidth = false; hg.childForceExpandWidth = false;

            var dot = UI("Dot", row);
            dot.gameObject.AddComponent<LayoutElement>().preferredWidth = 20f;
            var dT = dot.gameObject.AddComponent<TextMeshProUGUI>();
            dT.text = ">"; dT.fontSize = 16f;
            dT.color = C_GOLD; dT.alignment = TextAlignmentOptions.Center;

            var nm = UI("Name", row);
            nm.gameObject.AddComponent<LayoutElement>().preferredWidth = 360f;
            var nT = nm.gameObject.AddComponent<TextMeshProUGUI>();
            nT.text = pName; nT.fontSize = 21f;
            nT.fontStyle = FontStyles.Bold; nT.color = C_TEXT;
            nT.alignment = TextAlignmentOptions.MidlineLeft;
            nT.overflowMode = TextOverflowModes.Ellipsis;

            string cap = pName;
            btn.onClick.AddListener(() => SelectPlayer(cap));
            rowButtons.Add(btn);
        }

        // Chain row buttons → newPilot → back with explicit navigation
        var allExisting = new List<Button>(rowButtons);
        allExisting.AddRange(existingPanelButtons);
        SetupNavigation(allExisting);
    }

    // ═════════════════════════════════════════════════════════════════════════
    //  PANEL SHOW / HIDE
    // ═════════════════════════════════════════════════════════════════════════
    void ShowNewPanel()
    {
        newNamePanel?.SetActive(true);
        existingPlayersPanel?.SetActive(false);
        errorText?.gameObject.SetActive(false);

        if (nameInputField != null)
        {
            if (LeaderboardManager.Instance != null)
                nameInputField.text = LeaderboardManager.Instance.GetLastPlayerName();
            // Don't auto-activate input — let controller select the button first
        }

        // Auto-select LAUNCH MISSION button for controller
        StartCoroutine(SelectAfterDelay(confirmButton));
    }

    void ShowExistingPanel()
    {
        newNamePanel?.SetActive(false);
        existingPlayersPanel?.SetActive(true);
        PopulatePlayerList();

        // Auto-select first player row or newPilot button for controller
        StartCoroutine(SelectFirstExistingButton());
    }

    private IEnumerator SelectAfterDelay(Button btn)
    {
        yield return new WaitForSecondsRealtime(0.3f);

        if (GameInput.Instance != null)
            GameInput.Instance.EnableSubmitAction();

        if (EventSystem.current != null && btn != null)
            EventSystem.current.SetSelectedGameObject(btn.gameObject);
    }

    private IEnumerator SelectFirstExistingButton()
    {
        yield return new WaitForSecondsRealtime(0.3f);

        if (GameInput.Instance != null)
            GameInput.Instance.EnableSubmitAction();

        // Select first player row if it exists, otherwise newPilot button
        Button toSelect = newPilotButton;
        if (playerListContainer != null && playerListContainer.childCount > 0)
        {
            var firstRow = playerListContainer.GetChild(0)?.GetComponent<Button>();
            if (firstRow != null) toSelect = firstRow;
        }

        if (EventSystem.current != null && toSelect != null)
            EventSystem.current.SetSelectedGameObject(toSelect.gameObject);
    }

    // ═════════════════════════════════════════════════════════════════════════
    //  LOGIC
    // ═════════════════════════════════════════════════════════════════════════
    void SelectPlayer(string name)
    {
        LeaderboardManager.Instance?.SetCurrentPlayer(name);
        StartCoroutine(Proceed());
    }

    void OnConfirm()
    {
        if (nameInputField == null) return;
        string n = nameInputField.text.Trim();
        if (string.IsNullOrEmpty(n))
        { ShowError("You must enter a callsign to continue!"); return; }
        if (n.Length < 2)
        { ShowError("Callsign must be at least 2 characters!"); return; }
        errorText?.gameObject.SetActive(false);
        LeaderboardManager.Instance?.SetCurrentPlayer(n);
        StartCoroutine(Proceed());
    }

    void ShowError(string msg)
    {
        if (errorText == null) return;
        errorText.text = "!  " + msg;
        errorText.gameObject.SetActive(true);
    }

    void GoBackToMainMenu()
    {
        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(null);
        if (GameInput.Instance != null)
            GameInput.Instance.DisableSubmitAction();
        StartCoroutine(BackToMainMenuAfterDelay());
    }

    IEnumerator Proceed()
    {
        GameInput.Instance?.DisableSubmitAction();
        yield return new WaitForSecondsRealtime(0.15f);
        SceneLoader.LoadScene(SceneLoader.Scene.LevelSelectionScene);
    }

    IEnumerator BackToMainMenuAfterDelay()
    {
        yield return new WaitForSecondsRealtime(0.2f);
        SceneLoader.LoadScene(SceneLoader.Scene.MainMenuScene);
    }
}
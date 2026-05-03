using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AchievementUI : MonoBehaviour
{
    public static AchievementUI Instance { get; private set; }

    [Header("Pre-Level Panel")]
    [SerializeField] private GameObject preLevelPanel;
    [SerializeField] private Transform preLevelContainer;
    [SerializeField] private GameObject achievementRowPrefab;
    [SerializeField] private TextMeshProUGUI preLevelTitleText;

    private GameObject postLandingPanel;
    private CanvasGroup preLevelCanvasGroup;

    // Colours
    static readonly Color C_GOLD = new Color(1.00f, 0.82f, 0.22f, 1f);
    static readonly Color C_BLUE = new Color(0.20f, 0.55f, 1.00f, 1f);
    static readonly Color C_TEXT = new Color(0.95f, 0.97f, 1.00f, 1f);
    static readonly Color C_MUTED = new Color(0.65f, 0.75f, 0.88f, 1f);
    static readonly Color C_PANEL = new Color(0.04f, 0.08f, 0.18f, 0.96f);
    static readonly Color C_BORDER = new Color(0.25f, 0.55f, 1.00f, 0.90f);
    static readonly Color C_SHADOW = new Color(0.00f, 0.00f, 0.00f, 0.60f);
    static readonly Color C_ROW_OK = new Color(0.05f, 0.22f, 0.05f, 0.92f);
    static readonly Color C_ROW_NO = new Color(0.07f, 0.07f, 0.16f, 0.92f);

    private void Awake()
    {
        Instance = this;
        if (preLevelPanel != null)
        {
            preLevelCanvasGroup = preLevelPanel.GetComponent<CanvasGroup>()
                               ?? preLevelPanel.AddComponent<CanvasGroup>();
            preLevelPanel.SetActive(false);
        }
    }

    private void OnDestroy()
    {
        // Clean up dynamically created panel when scene unloads
        HidePostLanding();
        Instance = null;
    }

    // ── Pre-Level ─────────────────────────────────────────────────────────
    public void ShowPreLevelAchievements(int levelNumber, System.Action onDismiss)
    {
        if (preLevelPanel == null || AchievementManager.Instance == null) return;
        var ach = AchievementManager.Instance.GetAchievementsForLevel(levelNumber);
        if (preLevelTitleText != null) preLevelTitleText.text = $"LEVEL {levelNumber} CHALLENGES";
        if (preLevelContainer != null)
        {
            foreach (Transform c in preLevelContainer) Destroy(c.gameObject);
            foreach (var a in ach)
            {
                if (achievementRowPrefab == null) break;
                SetupPrefabRow(Instantiate(achievementRowPrefab, preLevelContainer), a);
            }
        }
        preLevelPanel.SetActive(true);
        if (preLevelCanvasGroup != null)
        {
            preLevelCanvasGroup.alpha = 1f;
            preLevelCanvasGroup.interactable = false;
            preLevelCanvasGroup.blocksRaycasts = false;
        }
    }

    public void HidePreLevel() => preLevelPanel?.SetActive(false);

    // ── Post-Landing ──────────────────────────────────────────────────────
    public void ShowPostLandingAchievements(List<Achievement> achievements,
        int totalStars, System.Action onContinue)
    {
        HidePostLanding();
        var canvas = FindFirstObjectByType<Canvas>();
        if (canvas != null)
            postLandingPanel = BuildPanel(
                canvas.GetComponent<RectTransform>(), achievements);
        onContinue?.Invoke();
    }

    public void HidePostLanding()
    {
        if (postLandingPanel != null)
        {
            Destroy(postLandingPanel);
            postLandingPanel = null;
        }
    }

    // ═════════════════════════════════════════════════════════════════════
    //  BUILD PANEL
    // ═════════════════════════════════════════════════════════════════════
    private GameObject BuildPanel(RectTransform canvas, List<Achievement> achievements)
    {
        int newCount = 0;
        foreach (var a in achievements) if (a.isCompleted) newCount++;

        const float PW = 500f;  // panel width
        const float PAD_X = 14f;   // horizontal padding
        const float PAD_TOP = 12f;
        const float PAD_BOT = 14f;
        const float HDR_H = 32f;   // header height
        const float DIV_H = 2f;    // divider height
        const float GAP = 6f;    // gap between elements
        const float ROW_H = 72f;   // each achievement row height

        float innerW = PW - PAD_X * 2f;

        // Total height = top pad + header + gap + divider + gap + (N rows + gaps) + bottom pad
        float totalH = PAD_TOP
                     + HDR_H + GAP
                     + DIV_H + GAP
                     + achievements.Count * ROW_H
                     + Mathf.Max(0, achievements.Count - 1) * GAP
                     + PAD_BOT;

        // ── Root ──────────────────────────────────────────────────────────
        var rootRT = NewRT("AchievementPanel", canvas);
        rootRT.anchorMin = new Vector2(0.5f, 1f);
        rootRT.anchorMax = new Vector2(0.5f, 1f);
        rootRT.pivot = new Vector2(0.5f, 1f);
        rootRT.sizeDelta = new Vector2(PW, totalH);
        rootRT.anchoredPosition = new Vector2(0f, -128f);

        // ── Shadow ────────────────────────────────────────────────────────
        var shadowRT = NewRT("Shadow", rootRT);
        shadowRT.gameObject.AddComponent<Image>().color = C_SHADOW;
        Expand(shadowRT, 7f);

        // ── Border ────────────────────────────────────────────────────────
        var borderRT = NewRT("Border", rootRT);
        borderRT.gameObject.AddComponent<Image>().color = C_BORDER;
        Expand(borderRT, 3f);

        // ── Panel ─────────────────────────────────────────────────────────
        var panelRT = NewRT("Panel", rootRT);
        panelRT.gameObject.AddComponent<Image>().color = C_PANEL;
        FillExact(panelRT);

        // ── Header ────────────────────────────────────────────────────────
        float cursorY = PAD_TOP;
        PutText(panelRT, "Header",
            newCount > 0
                ? $"ACHIEVEMENTS  [{newCount}/{achievements.Count} COMPLETED]"
                : $"ACHIEVEMENTS  [0/{achievements.Count} COMPLETED]",
            PAD_X, cursorY, innerW, HDR_H,
            17f, C_GOLD, FontStyles.Bold, TextAlignmentOptions.Center);
        cursorY += HDR_H + GAP;

        // ── Divider ───────────────────────────────────────────────────────
        var divRT = NewRT("Div", panelRT);
        SetRect(divRT, PAD_X, cursorY, innerW, DIV_H);
        divRT.gameObject.AddComponent<Image>().color =
            new Color(C_BLUE.r, C_BLUE.g, C_BLUE.b, 0.4f);
        cursorY += DIV_H + GAP;

        // ── Rows ──────────────────────────────────────────────────────────
        for (int i = 0; i < achievements.Count; i++)
        {
            PutRow(panelRT, achievements[i], PAD_X, cursorY, innerW, ROW_H);
            cursorY += ROW_H + (i < achievements.Count - 1 ? GAP : 0f);
        }

        return rootRT.gameObject;
    }

    // ── One achievement row ────────────────────────────────────────────────
    private void PutRow(RectTransform parent, Achievement a,
        float x, float y, float w, float h)
    {
        // Background
        var rowRT = NewRT($"Row_{a.id}", parent);
        SetRect(rowRT, x, y, w, h);
        rowRT.gameObject.AddComponent<Image>().color =
            a.isCompleted ? C_ROW_OK : C_ROW_NO;

        // Left badge: "★" or "○" using ASCII-safe characters
        string badge = a.isCompleted ? "[*]" : "[ ]";
        Color badgeCol = a.isCompleted ? C_GOLD : new Color(0.45f, 0.45f, 0.45f);
        float badgeW = 38f;

        // Right status label if completed
        string statusTxt = a.isCompleted ? "DONE" : "";
        float statusW = a.isCompleted ? 46f : 0f;

        float textX = badgeW + 4f;
        float textW = w - textX - statusW - 6f;

        // Badge — left side
        PutText(rowRT, "Badge", badge,
            4f, 0f, badgeW, h,
            15f, badgeCol,
            FontStyles.Bold, TextAlignmentOptions.Center);

        // Status — right side
        if (a.isCompleted)
        {
            PutText(rowRT, "Status", statusTxt,
                w - statusW - 4f, 0f, statusW, h,
                11f, new Color(0.3f, 1f, 0.3f),
                FontStyles.Bold, TextAlignmentOptions.Center);
        }

        // Title + description stacked in the middle
        float titleH = h * 0.50f;
        float descH = h * 0.36f;
        float titleY = (h - titleH - descH) / 2f;
        float descY = titleY + titleH + 2f;

        PutText(rowRT, "Title", a.title,
            textX, titleY, textW, titleH,
            17f, a.isCompleted ? new Color(0.88f, 1f, 0.88f) : C_TEXT,
            FontStyles.Bold, TextAlignmentOptions.MidlineLeft);

        PutText(rowRT, "Desc", a.description,
            textX, descY, textW, descH,
            13f, C_MUTED,
            FontStyles.Normal, TextAlignmentOptions.MidlineLeft);
    }

    // ── Text placement: all coordinates from TOP-LEFT of parent ───────────
    // x,y = distance from top-left; w,h = size in pixels
    private static void PutText(RectTransform parent, string name, string text,
        float x, float y, float w, float h,
        float fontSize, Color color, FontStyles style, TextAlignmentOptions align)
    {
        var rt = NewRT(name, parent);
        // Anchor to top-left so pixel offsets work correctly
        rt.anchorMin = new Vector2(0f, 1f);
        rt.anchorMax = new Vector2(0f, 1f);
        rt.pivot = new Vector2(0f, 1f);
        rt.sizeDelta = new Vector2(w, h);
        rt.anchoredPosition = new Vector2(x, -y);   // negative Y = down from top

        var tmp = rt.gameObject.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.fontStyle = style;
        tmp.color = color;
        tmp.alignment = align;
        tmp.overflowMode = TextOverflowModes.Ellipsis;
        tmp.enableWordWrapping = false;
    }

    // ── Rect helpers ──────────────────────────────────────────────────────

    // Place a rect anchored to top-left of parent at pixel x,y with size w,h
    private static void SetRect(RectTransform rt, float x, float y, float w, float h)
    {
        rt.anchorMin = new Vector2(0f, 1f);
        rt.anchorMax = new Vector2(0f, 1f);
        rt.pivot = new Vector2(0f, 1f);
        rt.sizeDelta = new Vector2(w, h);
        rt.anchoredPosition = new Vector2(x, -y);
    }

    // Stretch to fill parent exactly
    private static void FillExact(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
    }

    // Expand beyond parent edges by `amount` pixels on all sides
    private static void Expand(RectTransform rt, float amount)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = new Vector2(-amount, -amount);
        rt.offsetMax = new Vector2(amount, amount);
    }

    private static RectTransform NewRT(string name, Transform parent)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        return go.AddComponent<RectTransform>();
    }

    // ── Pre-level prefab row setup ─────────────────────────────────────────
    private void SetupPrefabRow(GameObject row, Achievement a)
    {
        if (row == null) return;
        var t = row.transform.Find("TitleText")?.GetComponent<TextMeshProUGUI>();
        var d = row.transform.Find("DescText")?.GetComponent<TextMeshProUGUI>();
        var s = row.transform.Find("StarIcon")?.GetComponent<Image>();
        var c = row.transform.Find("CheckIcon")?.gameObject;
        if (t != null) t.text = a.title;
        if (d != null) d.text = a.description;
        if (s != null) s.color = a.isCompleted ? C_GOLD : new Color(0.4f, 0.4f, 0.4f);
        if (c != null) c.SetActive(a.isCompleted);
        var bg = row.GetComponent<Image>();
        if (bg != null) bg.color = a.isCompleted ? C_ROW_OK : C_ROW_NO;
    }
}
using System.Collections;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Attach to any Button to get:
/// - 3D shadow effect (always visible, deepens on press)
/// - Glow/highlight when selected by controller or hovered by mouse
/// - Tick sound on hover/select, confirm sound on click
/// </summary>
[RequireComponent(typeof(Button))]
public class ButtonEffects : MonoBehaviour, ISelectHandler, IDeselectHandler,
    IPointerEnterHandler, IPointerExitHandler,
    IPointerDownHandler, IPointerUpHandler, ISubmitHandler
{
    [Header("Shadow Settings")]
    [SerializeField] private Vector2 shadowNormalOffset = new Vector2(4f, -4f);
    [SerializeField] private Vector2 shadowPressedOffset = new Vector2(1f, -1f);
    [SerializeField] private Color shadowColor = new Color(0f, 0f, 0f, 0.5f);

    [Header("Glow / Outline Settings")]
    [SerializeField] private Color normalOutlineColor = new Color(0f, 0f, 0f, 0f);
    [SerializeField] private Color selectedOutlineColor = new Color(1f, 0.85f, 0.2f, 1f); // gold
    [SerializeField] private Color pressedOutlineColor = new Color(1f, 1f, 1f, 1f);
    [SerializeField] private float outlineWidth = 3f;

    [Header("Scale Animation")]
    [SerializeField] private float hoverScale = 1.07f;
    [SerializeField] private float pressedScale = 0.94f;
    [SerializeField] private float scaleSpeed = 12f;

    [Header("Sounds")]
    [SerializeField] private AudioClip hoverSound;   // tick sound on hover/select
    [SerializeField] private AudioClip clickSound;   // confirm sound on click/submit
    [SerializeField][Range(0f, 1f)] private float soundVolume = 0.7f;

    // ── internal state ──────────────────────────────────────────────
    private Button button;
    private Shadow shadowComponent;
    private Outline outlineComponent;
    private RectTransform rectTransform;

    private bool isSelected = false;
    private bool isPressed = false;
    private Vector3 targetScale = Vector3.one;

    private static AudioSource sharedAudioSource; // one shared source for all buttons

    // ────────────────────────────────────────────────────────────────
    private void Awake()
    {
        button = GetComponent<Button>();
        rectTransform = GetComponent<RectTransform>();

        SetupShadow();
        SetupOutline();
        EnsureAudioSource();
    }

    private void SetupShadow()
    {
        // Add Shadow component for 3D depth look
        shadowComponent = GetComponent<Shadow>();
        if (shadowComponent == null)
            shadowComponent = gameObject.AddComponent<Shadow>();

        shadowComponent.effectColor = shadowColor;
        shadowComponent.effectDistance = shadowNormalOffset;
        shadowComponent.useGraphicAlpha = true;
    }

    private void SetupOutline()
    {
        // Add Outline component for selection glow
        outlineComponent = GetComponent<Outline>();
        if (outlineComponent == null)
            outlineComponent = gameObject.AddComponent<Outline>();

        outlineComponent.effectColor = normalOutlineColor;
        outlineComponent.effectDistance = new Vector2(outlineWidth, -outlineWidth);
        outlineComponent.useGraphicAlpha = false;
    }

    private void EnsureAudioSource()
    {
        if (sharedAudioSource == null)
        {
            GameObject audioObj = new GameObject("ButtonAudioSource");
            DontDestroyOnLoad(audioObj);
            sharedAudioSource = audioObj.AddComponent<AudioSource>();
            sharedAudioSource.playOnAwake = false;
            sharedAudioSource.spatialBlend = 0f; // 2D sound
        }
    }

    // ── Update: smooth scale ─────────────────────────────────────────
    private void Update()
    {
        if (!button.interactable)
        {
            transform.localScale = Vector3.one;
            return;
        }

        transform.localScale = Vector3.Lerp(
            transform.localScale,
            targetScale,
            Time.unscaledDeltaTime * scaleSpeed
        );
    }

    // ── ISelectHandler: controller navigates to this button ──────────
    public void OnSelect(BaseEventData eventData)
    {
        if (!button.interactable) return;

        isSelected = true;
        UpdateVisuals();
        PlaySound(hoverSound);
    }

    public void OnDeselect(BaseEventData eventData)
    {
        isSelected = false;
        if (!isPressed) UpdateVisuals();
    }

    // ── IPointerEnterHandler: mouse hovers ───────────────────────────
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!button.interactable) return;

        isSelected = true;
        UpdateVisuals();
        PlaySound(hoverSound);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isSelected = false;
        if (!isPressed) UpdateVisuals();
    }

    // ── IPointerDownHandler / Up: mouse click ────────────────────────
    public void OnPointerDown(PointerEventData eventData)
    {
        if (!button.interactable) return;

        isPressed = true;
        UpdateVisuals();
        PlaySound(clickSound);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isPressed = false;
        UpdateVisuals();
    }

    // ── ISubmitHandler: controller presses Cross/Enter ───────────────
    public void OnSubmit(BaseEventData eventData)
    {
        if (!button.interactable) return;

        PlaySound(clickSound);
        StartCoroutine(PressAnimation());
    }

    private IEnumerator PressAnimation()
    {
        isPressed = true;
        UpdateVisuals();
        yield return new WaitForSecondsRealtime(0.1f);
        isPressed = false;
        UpdateVisuals();
    }

    // ── Visual update ────────────────────────────────────────────────
    private void UpdateVisuals()
    {
        if (isPressed)
        {
            // Pressed state
            targetScale = Vector3.one * pressedScale;
            shadowComponent.effectDistance = shadowPressedOffset;
            outlineComponent.effectColor = pressedOutlineColor;
        }
        else if (isSelected)
        {
            // Hover / selected state
            targetScale = Vector3.one * hoverScale;
            shadowComponent.effectDistance = shadowNormalOffset;
            outlineComponent.effectColor = selectedOutlineColor;
        }
        else
        {
            // Normal state
            targetScale = Vector3.one;
            shadowComponent.effectDistance = shadowNormalOffset;
            outlineComponent.effectColor = normalOutlineColor;
        }
    }

    // ── Sound ────────────────────────────────────────────────────────
    private void PlaySound(AudioClip clip)
    {
        if (clip == null || sharedAudioSource == null) return;
        sharedAudioSource.PlayOneShot(clip, soundVolume);
    }

    // ── Cleanup on destroy ───────────────────────────────────────────
    private void OnDestroy()
    {
        // Reset scale in case object is pooled
        transform.localScale = Vector3.one;
    }
}
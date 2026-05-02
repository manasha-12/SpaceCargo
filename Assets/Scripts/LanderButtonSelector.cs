using UnityEngine;
using UnityEngine.EventSystems;

public class LanderButtonSelector : MonoBehaviour, ISubmitHandler, IPointerClickHandler
{
    [SerializeField] private LanderSelectionManager.LanderType landerType;
    private LanderSelectionUI landerSelectionUI;

    private float lastFireTime = -1f;
    private const float COOLDOWN = 0.4f;

    private void Start()
    {
        landerSelectionUI = FindFirstObjectByType<LanderSelectionUI>();
    }

    private bool CanFire()
    {
        if (Time.unscaledTime - lastFireTime < COOLDOWN) return false;
        lastFireTime = Time.unscaledTime;
        return true;
    }

    // Controller press
    public void OnSubmit(BaseEventData eventData)
    {
        if (!CanFire()) return;
        landerSelectionUI.SelectLanderPublic(landerType);
    }

    // Mouse click
    public void OnPointerClick(PointerEventData eventData)
    {
        if (!CanFire()) return;
        landerSelectionUI.SelectLanderPublic(landerType);
    }
}
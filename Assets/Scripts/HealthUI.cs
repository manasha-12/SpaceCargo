using UnityEngine;
using UnityEngine.UI;

public class HealthUI : MonoBehaviour
{
    [Header("Health Bar")]
    [SerializeField] private Image healthBarFill;

    [Header("Heart Images (Optional)")]
    [SerializeField] private Image[] heartImages;
    [SerializeField] private Sprite fullHeart;
    [SerializeField] private Sprite emptyHeart;

    [Header("Colors")]
    [SerializeField] private Color healthyColor = Color.green;
    [SerializeField] private Color damagedColor = Color.yellow;
    [SerializeField] private Color criticalColor = Color.red;

    private void Start()
    {
        Debug.Log("HealthUI: Start() called");
        SubscribeToLander();
    }

    private void OnEnable()
    {
        // Re-subscribe when UI is enabled (scene reload, etc.)
        Debug.Log("HealthUI: OnEnable() called");
        SubscribeToLander();
    }

    private void SubscribeToLander()
    {
        if (Lander.Instance == null)
        {
            Debug.LogWarning("HealthUI: Lander.Instance is NULL! Retrying in 0.1s...");
            Invoke(nameof(SubscribeToLander), 0.1f);
            return;
        }

        // Unsubscribe first to prevent double subscription
        Lander.Instance.OnHealthChanged -= Lander_OnHealthChanged;

        // Subscribe
        Lander.Instance.OnHealthChanged += Lander_OnHealthChanged;
        Debug.Log("HealthUI: Subscribed to Lander.OnHealthChanged");

        // Initialize health display
        UpdateHealthBar(Lander.Instance.GetHealthNormalized());
        UpdateHearts(Lander.Instance.GetCurrentHealth(), Lander.Instance.GetMaxHealth());

        Debug.Log($"HealthUI: Initialized with health {Lander.Instance.GetCurrentHealth()}/{Lander.Instance.GetMaxHealth()}");
    }

    private void Lander_OnHealthChanged(object sender, Lander.OnHealthChangedEventArgs e)
    {
        Debug.Log($"HealthUI: Health changed event received! Current: {e.currentHealth}, Max: {e.maxHealth}, Damage: {e.damage}");

        float healthPercent = (float)e.currentHealth / e.maxHealth;

        UpdateHealthBar(healthPercent);
        UpdateHearts(e.currentHealth, e.maxHealth);

        if (e.damage > 0)
        {
            StartCoroutine(FlashHealthBar());
        }
    }

    private void UpdateHealthBar(float healthPercent)
    {
        if (healthBarFill != null)
        {
            healthBarFill.fillAmount = healthPercent;

            // Change color based on health
            if (healthPercent > 0.66f)
            {
                healthBarFill.color = healthyColor;
            }
            else if (healthPercent > 0.33f)
            {
                healthBarFill.color = damagedColor;
            }
            else
            {
                healthBarFill.color = criticalColor;
            }

            Debug.Log($"HealthUI: Updated health bar to {healthPercent * 100}%");
        }
    }

    private void UpdateHearts(int currentHealth, int maxHealth)
    {
        if (heartImages == null || heartImages.Length == 0)
        {
            Debug.Log("HealthUI: No heart images configured");
            return;
        }

        Debug.Log($"HealthUI: Updating hearts - {currentHealth}/{maxHealth}");

        for (int i = 0; i < heartImages.Length; i++)
        {
            if (i < maxHealth)
            {
                heartImages[i].gameObject.SetActive(true);

                if (i < currentHealth)
                {
                    // Full heart
                    if (fullHeart != null)
                        heartImages[i].sprite = fullHeart;
                    heartImages[i].color = Color.white;
                }
                else
                {
                    // Empty heart
                    if (emptyHeart != null)
                        heartImages[i].sprite = emptyHeart;
                    heartImages[i].color = Color.gray;
                }
            }
            else
            {
                heartImages[i].gameObject.SetActive(false);
            }
        }
    }

    private System.Collections.IEnumerator FlashHealthBar()
    {
        if (healthBarFill == null) yield break;

        Color originalColor = healthBarFill.color;

        // Flash white
        healthBarFill.color = Color.white;
        yield return new WaitForSeconds(0.1f);

        healthBarFill.color = originalColor;
    }

    private void OnDisable()
    {
        Debug.Log("HealthUI: OnDisable() called");
    }

    private void OnDestroy()
    {
        Debug.Log("HealthUI: OnDestroy() called");

        if (Lander.Instance != null)
        {
            Lander.Instance.OnHealthChanged -= Lander_OnHealthChanged;
            Debug.Log("HealthUI: Unsubscribed from health events");
        }
    }
}
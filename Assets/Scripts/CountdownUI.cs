using System.Collections;
using TMPro;
using UnityEngine;

public class CountdownUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI countdownText;
    [SerializeField] private float countdownDuration = 2f;

    [Header("Sound Effects")]
    [SerializeField] private AudioClip goSound;

    private bool isCountingDown = false;
    private Vector3 originalScale;

    private void Start()
    {
        // Store original scale for animations
        originalScale = countdownText.transform.localScale;

        // Listen to lander state changes
        if (Lander.Instance != null)
        {
            Lander.Instance.OnStateChange += Lander_OnStateChange;
        }

        // Listen to game pause/unpause
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGameUnPaused += GameManager_OnGameUnPaused;
        }

        Hide();
    }

    private void Lander_OnStateChange(object sender, Lander.OnStateChangedEventAgrs e)
    {
        if (e.state == Lander.State.Normal && !isCountingDown)
        {
            StartCoroutine(ShowCountdown());
        }
    }

    private void GameManager_OnGameUnPaused(object sender, System.EventArgs e)
    {
        StartCoroutine(ShowCountdown());
    }

    private IEnumerator ShowCountdown()
    {
        if (isCountingDown) yield break;

        isCountingDown = true;
        Show();

        // Pause the game during countdown
        Time.timeScale = 0f;

        // Countdown from 3 to 1 with pulse animation
        for (int i = (int)countdownDuration; i > 0; i--)
        {
            countdownText.text = i.ToString();


            // Pulse animation
            float timer = 0f;
            while (timer < 1f)
            {
                timer += Time.unscaledDeltaTime;

                // Scale pulse: big → small
                float pulse = 1f - timer;
                float scale = Mathf.Lerp(1f, 1.5f, pulse);
                countdownText.transform.localScale = originalScale * scale;

                // Color pulse: white → yellow
                Color color = Color.Lerp(Color.yellow, Color.white, pulse);
                countdownText.color = color;

                // Size pulse
                float fontSize = Mathf.Lerp(120f, 180f, pulse);
                countdownText.fontSize = fontSize;

                yield return null;
            }

            // Reset to normal size
            countdownText.transform.localScale = originalScale;
        }

        // Show "GO!" with extra emphasis
        countdownText.text = "GO!";
        countdownText.fontSize = 250;
        countdownText.color = Color.yellow;
        countdownText.transform.localScale = originalScale * 1.8f;

        // Play go sound
        PlaySound(goSound);

        // Quick scale animation for GO
        float goTimer = 0f;
        while (goTimer < 0.5f)
        {
            goTimer += Time.unscaledDeltaTime;

            // Pulse GO text
            float pulse = Mathf.Sin(goTimer * Mathf.PI * 4f);
            float scale = 1.8f + (pulse * 0.2f);
            countdownText.transform.localScale = originalScale * scale;

            yield return null;
        }

        Hide();

        // Reset everything
        countdownText.transform.localScale = originalScale;
        countdownText.color = Color.white;

        // Resume the game
        Time.timeScale = 1f;
        isCountingDown = false;
    }

    private void PlaySound(AudioClip clip)
    {
        if (clip != null)
        {
            AudioSource.PlayClipAtPoint(clip, Camera.main.transform.position);
        }
    }

    private void Show()
    {
        countdownText.enabled = true;
    }

    private void Hide()
    {
        countdownText.enabled = false;
    }

    private void OnDestroy()
    {
        if (Lander.Instance != null)
        {
            Lander.Instance.OnStateChange -= Lander_OnStateChange;
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGameUnPaused -= GameManager_OnGameUnPaused;
        }
    }
}
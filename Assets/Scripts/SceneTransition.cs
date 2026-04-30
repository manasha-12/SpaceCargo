using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneTransition : MonoBehaviour
{
    public static SceneTransition Instance { get; private set; }

    [SerializeField] private Image fadeImage;
    [SerializeField] private float fadeDuration = 1f;
    [SerializeField] private AnimationCurve fadeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    private bool isTransitioning = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        if (fadeImage != null)
        {
            Color color = fadeImage.color;
            color.a = 0f;
            fadeImage.color = color;

            // Disable raycast when transparent
            fadeImage.raycastTarget = false; 
        }
    }

    public void FadeToScene(string sceneName)
    {
        if (!isTransitioning)
        {
            StartCoroutine(FadeAndLoadScene(sceneName));
        }
    }

    public void FadeOut(System.Action onComplete = null)
    {
        StartCoroutine(FadeCoroutine(0f, 1f, onComplete));
    }

    public void FadeIn(System.Action onComplete = null)
    {
        StartCoroutine(FadeCoroutine(1f, 0f, onComplete));
    }

    private IEnumerator FadeAndLoadScene(string sceneName)
    {
        isTransitioning = true;

        // Fade out
        yield return StartCoroutine(FadeCoroutine(0f, 1f));

        // Load scene
        SceneManager.LoadScene(sceneName);
        yield return null;

        // Fade in
        yield return StartCoroutine(FadeCoroutine(1f, 0f));

        isTransitioning = false;
    }

    private IEnumerator FadeCoroutine(float startAlpha, float endAlpha, System.Action onComplete = null)
    {
        Debug.Log($"Fade starting: {startAlpha} → {endAlpha}");

        // Enable raycast during fade (blocks clicks)
        if (fadeImage != null)
        {
            fadeImage.raycastTarget = true;
            Debug.Log("Raycast enabled");
        }
        else
        {
            Debug.LogError("FadeImage is NULL!");
            yield break;
        }

        float elapsedTime = 0f;
        Color color = fadeImage.color;
        Debug.Log($"Initial color: {color}");

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsedTime / fadeDuration);
            float curvedT = fadeCurve.Evaluate(t);

            color.a = Mathf.Lerp(startAlpha, endAlpha, curvedT);
            fadeImage.color = color;

            yield return null;
        }

        // Ensure final alpha is exact
        color.a = endAlpha;
        fadeImage.color = color;

        Debug.Log($"Fade complete: final alpha = {color.a}");

        // Disable raycast when fully transparent
        if (fadeImage != null && endAlpha == 0f)
        {
            fadeImage.raycastTarget = false;
            Debug.Log("Raycast disabled");
        }

        onComplete?.Invoke();
    }
}
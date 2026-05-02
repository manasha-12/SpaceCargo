using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class LandedUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI titleTextMesh;
    [SerializeField] private TextMeshProUGUI statsTextMesh;
    [SerializeField] private TextMeshProUGUI nextButtonTextMesh;
    [SerializeField] private Button nextButton;

    private Action nextButtonClickAction;

    private void Awake()
    {
        nextButton.onClick.AddListener(() =>
        {
            nextButtonClickAction?.Invoke();
        });
    }

    private void Start()
    {
        Lander.Instance.OnLanded += Lander_OnLanded;
        Hide();
    }

    private void Lander_OnLanded(object sender, Lander.OnLandedEventArgs e)
    {
        if (e.landingType == Lander.LandingType.Success)
        {
            titleTextMesh.text = "WOW PERFECT LANDING!";
            nextButtonTextMesh.text = "GO AHEAD";
            nextButtonClickAction = GameManager.Instance.GoToNextLevel;
        }
        else
        {
            // Check if all lives are exhausted
            bool isGameOver = GameManager.Instance != null
                              && GameManager.Instance.IsGameOver();

            if (isGameOver)
            {
                titleTextMesh.text = "GAME OVER!";
                nextButtonTextMesh.text = "SEE RESULTS";
                nextButtonClickAction = () =>
                    SceneLoader.LoadScene(SceneLoader.Scene.GameOverScene);
            }
            else
            {
                titleTextMesh.text = "OOPS CRASHED!";
                nextButtonTextMesh.text = "FLY AGAIN";
                nextButtonClickAction = GameManager.Instance.RetryCurrentLevel;
            }
        }

        statsTextMesh.text =
            Mathf.Round(e.landingSpeed * 2f) + "\n" +
            Mathf.Round(e.dotVector * 100f) + "\n" +
            "x" + e.scoreMultiplier + "\n" +
            e.score;

        Show();
        StartCoroutine(SelectButtonAfterDelay());
    }

    private IEnumerator SelectButtonAfterDelay()
    {
        yield return new WaitForSecondsRealtime(0.1f);
        if (nextButton != null && EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(nextButton.gameObject);
    }

    private void Show() => gameObject.SetActive(true);
    private void Hide() => gameObject.SetActive(false);
}
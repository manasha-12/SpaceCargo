using System;
using System.Collections;
using System.Collections.Generic;
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
            // Hide achievement panel at same time as proceeding
            AchievementUI.Instance?.HidePostLanding();
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
            SetStats(e);

            if (AchievementManager.Instance != null && GameManager.Instance != null)
            {
                int level = GameManager.Instance.GetLevelNumber();
                bool fullHealth = Lander.Instance.GetCurrentHealth()
                               >= Lander.Instance.GetMaxHealth();

                // Evaluate — marks which achievements were completed this run
                AchievementManager.Instance.EvaluateAchievements(
                    level, e.landingSpeed, e.score, fullHealth);

                // Get ALL achievements for this level (completed + not completed)
                // so the UI always shows the full list regardless of what was achieved
                List<Achievement> allAchievements =
                    AchievementManager.Instance.GetAchievementsForLevel(level);

                if (AchievementUI.Instance != null)
                {
                    AchievementUI.Instance.ShowPostLandingAchievements(
                        allAchievements,          // full list — always 3
                        AchievementManager.Instance.GetTotalStars(),
                        () =>
                        {
                            Show();
                            StartCoroutine(SelectButtonAfterDelay());
                        });
                    return;
                }
            }

            Show();
            StartCoroutine(SelectButtonAfterDelay());
        }
        else
        {
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

            SetStats(e);
            Show();
            StartCoroutine(SelectButtonAfterDelay());
        }
    }

    private void SetStats(Lander.OnLandedEventArgs e)
    {
        statsTextMesh.text =
            Mathf.Round(e.landingSpeed * 2f) + "\n" +
            Mathf.Round(e.dotVector * 100f) + "\n" +
            "x" + e.scoreMultiplier + "\n" +
            e.score;
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
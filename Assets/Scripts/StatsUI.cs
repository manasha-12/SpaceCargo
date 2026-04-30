using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StatsUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI statsTextMesh;
    [SerializeField] private TextMeshProUGUI levelTextMesh;
    [SerializeField] private Image fuelImage;

    private void Update()
    {
        UpdateStatsTextMesh();
    }

    private void UpdateStatsTextMesh()
    {
        if (Lander.Instance == null || GameManager.Instance == null) return;

        float fuelPercent = Lander.Instance.GetFuelAmountNormalized();
        fuelImage.fillAmount = fuelPercent;

        // Dynamic color based on fuel level
        if (fuelPercent < 0.25f)
            fuelImage.color = Color.red; // Critical!
        else if (fuelPercent < 0.5f)
            fuelImage.color = Color.yellow; // Warning
        else
            fuelImage.color = Color.green; // Good

        statsTextMesh.text =
            GameManager.Instance.GetScore() + "\n" +
            Mathf.Round(GameManager.Instance.GetTime()) + "\n" +
            Mathf.Round(Lander.Instance.GetSpeedX() * 10f) + "\n" +
            Mathf.Round(Lander.Instance.GetSpeedY() * 10f);
        levelTextMesh.text = GameManager.Instance.GetLevelNumber().ToString();
    }
}
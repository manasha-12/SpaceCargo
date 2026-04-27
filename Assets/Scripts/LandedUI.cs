using TMPro;
using UnityEngine;

public class LandedUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI titleTextMesh;
    [SerializeField] private TextMeshProUGUI statsTextMesh;\

    private void Start()
    {
        Lander.Instance.OnLanded += Lander_OnLanded;
    }

    private void Lander_OnLanded(object sender, Lander.OnLandedEventArgs e)
    {
        if (e.landingType == Lander.LandingType.Success)
        {
            titleTextMesh.text = "CONGRATS SUCCESSFUL LANDING!";
        } else
        {
            titleTextMesh.text = "OOPS CRASHED!";
        }

        statsTextMesh.text =
            Mathf.Round(e.landingSpeed) + "\n" +
            Mathf.Round(e.dotVector) + "\n" +
            "x" + e.scoreMultiplier + "\n" +
            e.score;

    }
}

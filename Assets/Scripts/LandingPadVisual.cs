using TMPro;
using UnityEngine;

public class LandingPadVisual : MonoBehaviour
{
    [SerializeField] private TextMeshPro scoreMultiplierTextMesh;

    private void Awake()
    {
        LandingPad landingPad = GetComponent<LandingPad>();

        if (landingPad != null)
        {
            int multiplier = landingPad.GetScoreMultiplier();
            scoreMultiplierTextMesh.text = "x" + multiplier;
            Debug.Log("Setting text to: x" + multiplier);
        }
        else
        {
            Debug.LogError("LandingPad component not found!");
        }
    }
}
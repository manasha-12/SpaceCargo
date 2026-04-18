using TMPro;
using UnityEngine;

public class StatsUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI statsTextMesh;

    private void UpdateStatsTextMesh()
    {
        statsTextMesh.text = GameManager.Instance.GetScore() + "\n"
            ;

    }
}

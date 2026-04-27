using UnityEngine;

public class GameLevel : MonoBehaviour
{
    [SerializeField] private int levelNumber;
    [SerializeField] private Transform landerStartPositionTransform;
    [SerializeField] private Transform cameraStartTargetTransform;

    public int GetLevelNumber()
    {
        return levelNumber;
    }

    public Vector3 GetLanderPosition()
    {
        return landerStartPositionTransform.position;
    }
}

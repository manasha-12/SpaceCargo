using UnityEngine;

public class MovingObstacle : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private Vector2 direction = Vector2.left;
    [SerializeField] private float rotationSpeed = 50f;
    [SerializeField] private float destroyDistance = 25f;

    private Vector3 startPosition;

    private void Start()
    {
        startPosition = transform.position;
    }

    private void Update()
    {
        // Move in direction
        transform.Translate(direction * moveSpeed * Time.deltaTime, Space.World);

        // Rotate
        transform.Rotate(Vector3.forward * rotationSpeed * Time.deltaTime);

        // Destroy if too far from start
        float distance = Vector3.Distance(startPosition, transform.position);
        if (distance > destroyDistance)
        {
            Destroy(gameObject);
        }
    }
}
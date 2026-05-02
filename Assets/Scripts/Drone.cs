using UnityEngine;

public class Drone : MonoBehaviour
{
    [Header("AI Settings")]
    [SerializeField] private float chaseSpeed = 3f;
    [SerializeField] private float rotationSpeed = 2f;
    [SerializeField] private float detectionRange = 30f;
    [SerializeField] private float stopDistance = 3f; // Don't get too close

    [Header("Combat")]
    [SerializeField] private int damageAmount = 1;
    [SerializeField] private bool destroyOnCollision = false;

    [Header("Visual")]
    [SerializeField] private GameObject explosionPrefab;

    private Transform target;
    private Rigidbody2D droneRigidbody;
    private bool isChasing = false;

    private void Awake()
    {
        droneRigidbody = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        // Find the lander
        if (Lander.Instance != null)
        {
            target = Lander.Instance.transform;
            isChasing = true;
        }
        else
        {
            Debug.LogWarning("Drone couldn't find Lander!");
        }
    }

    private void FixedUpdate()
    {
        if (!isChasing || target == null) return;

        float distanceToTarget = Vector2.Distance(transform.position, target.position);

        // Only chase if within detection range and not too close
        if (distanceToTarget < detectionRange && distanceToTarget > stopDistance)
        {
            ChaseTarget();
        }
        else if (distanceToTarget <= stopDistance)
        {
            // Stop moving when close enough
            droneRigidbody.linearVelocity = Vector2.zero;
        }
    }

    private void ChaseTarget()
    {
        // Calculate direction to target
        Vector2 direction = (target.position - transform.position).normalized;

        // Move towards target
        Vector2 targetVelocity = direction * chaseSpeed;
        droneRigidbody.linearVelocity = Vector2.Lerp(droneRigidbody.linearVelocity, targetVelocity, Time.fixedDeltaTime * 5f);

        // Rotate to face target
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
        Quaternion targetRotation = Quaternion.Euler(0, 0, angle);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Check if hit the lander
        if (collision.gameObject.TryGetComponent(out Lander lander))
        {
            Debug.Log("Drone hit the lander!");

            // Damage the lander
            lander.TakeDamage(damageAmount);

            // Destroy drone if configured
            if (destroyOnCollision)
            {
                DestroyDrone();
            }
        }
    }

    private void DestroyDrone()
    {
        // Spawn explosion effect
        if (explosionPrefab != null)
        {
            Instantiate(explosionPrefab, transform.position, Quaternion.identity);
        }
        AchievementManager.Instance?.OnDroneKilled();
        Destroy(gameObject);
    }

    // Optional: Allow player to destroy drone
    public void TakeDamage()
    {
        DestroyDrone();
    }

    // Gizmo for debugging detection range
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, stopDistance);
    }

    public void TakeDamage(int damage)
    {
        Debug.Log("Drone destroyed by projectile!");
        DestroyDrone();
    }
}
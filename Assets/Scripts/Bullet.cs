using UnityEngine;

public class Bullet : MonoBehaviour
{
    [Header("Bullet Settings")]
    [SerializeField] private float speed = 15f;
    [SerializeField] private float lifetime = 3f;
    [SerializeField] private int damage = 1;

    [Header("Effects")]
    [SerializeField] private GameObject hitEffectPrefab;

    private Rigidbody2D bulletRigidbody;

    private void Awake()
    {
        bulletRigidbody = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        // Set velocity in the direction the bullet is facing
        bulletRigidbody.linearVelocity = transform.up * speed;

        // Destroy after lifetime expires
        Destroy(gameObject, lifetime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Check if hit a drone
        if (collision.gameObject.TryGetComponent(out Drone drone))
        {
            Debug.Log("Bullet hit drone!");

            // Damage the drone
            drone.TakeDamage(damage);

            // Spawn hit effect
            SpawnHitEffect();

            // Destroy bullet
            Destroy(gameObject);
            return;
        }

        // Check if hit terrain or asteroid (destroy bullet)
        if (collision.gameObject.CompareTag("Terrain") ||
            collision.gameObject.CompareTag("Asteroid") ||
            collision.gameObject.layer == LayerMask.NameToLayer("Terrain"))
        {
            Debug.Log("Bullet hit obstacle!");
            SpawnHitEffect();
            Destroy(gameObject);
            return;
        }
    }

    private void SpawnHitEffect()
    {
        if (hitEffectPrefab != null)
        {
            Instantiate(hitEffectPrefab, transform.position, Quaternion.identity);
        }
    }
}
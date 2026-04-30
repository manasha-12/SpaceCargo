using UnityEngine;

public class SlowFallPowerUp : MonoBehaviour
{
    [SerializeField] private float duration = 5f;
    [SerializeField] private float reducedGravity = 0.3f;
    [SerializeField] private ParticleSystem collectionEffect;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent(out Lander lander))
        {
            // Apply slow fall effect
            lander.ApplySlowFall(duration, reducedGravity);

            // Visual feedback
            if (collectionEffect != null)
            {
                Instantiate(collectionEffect, transform.position, Quaternion.identity);
            }

            // Destroy power-up
            Destroy(gameObject);
        }
    }
}
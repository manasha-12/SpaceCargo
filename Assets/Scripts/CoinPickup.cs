using UnityEngine;

public class CoinPickup : MonoBehaviour
{
    [SerializeField] private ParticleSystem collectionEffect;
    [SerializeField] private AudioClip collectionSound; // Add this

    public void DestroySelf()
    {
        // Spawn particle effect
        if (collectionEffect != null)
        {
            Instantiate(collectionEffect, transform.position, Quaternion.identity);
        }

        // Play sound effect
        if (collectionSound != null)
        {
            AudioSource.PlayClipAtPoint(collectionSound, transform.position);
        }

        Destroy(gameObject);
    }
}
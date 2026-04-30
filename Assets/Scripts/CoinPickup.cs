using UnityEngine;

public class CoinPickup : MonoBehaviour
{
    [SerializeField] private ParticleSystem collectionEffect;

    public void DestroySelf()
    {
        // Spawn particle effect before destroying
        if (collectionEffect != null)
        {
            Instantiate(collectionEffect, transform.position, Quaternion.identity);
        }

        Destroy(gameObject);
    }
}
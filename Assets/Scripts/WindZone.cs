using UnityEngine;

public class WindZone : MonoBehaviour
{
    [SerializeField] private Vector2 windForce = new Vector2(5f, 0f);
    [SerializeField] private ParticleSystem windParticles;
    [SerializeField] private bool showDebugGizmo = true;

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.TryGetComponent(out Rigidbody2D rb))
        {
            rb.AddForce(windForce * Time.fixedDeltaTime);
        }
    }

    private void OnDrawGizmos()
    {
        if (showDebugGizmo)
        {
            Gizmos.color = new Color(0f, 1f, 1f, 0.3f);
            BoxCollider2D boxCollider = GetComponent<BoxCollider2D>();
            if (boxCollider != null)
            {
                Gizmos.DrawCube(transform.position, boxCollider.size);

                // Draw wind direction arrow
                Gizmos.color = Color.cyan;
                Vector3 arrowEnd = transform.position + (Vector3)windForce.normalized * 2f;
                Gizmos.DrawLine(transform.position, arrowEnd);
            }
        }
    }
}
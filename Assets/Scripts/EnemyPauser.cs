using UnityEngine;

/// <summary>
/// Add this component to any enemy or hazard GameObject (Asteroid, Drone, etc).
/// It freezes the Rigidbody2D and disables Collider2Ds while the game is not active,
/// so they cannot damage the lander before the player starts or after landing.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class EnemyPauser : MonoBehaviour
{
    private Rigidbody2D rb;
    private Collider2D[] cols;

    // Store velocity so we can restore it when unpausing
    private Vector2 savedVelocity;
    private float savedAngularVelocity;
    private float savedGravityScale;
    private bool isPaused = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        cols = GetComponentsInChildren<Collider2D>();
    }

    private void Update()
    {
        bool shouldBePaused = !GameStateManager.IsGameActive;

        if (shouldBePaused && !isPaused)
            Pause();
        else if (!shouldBePaused && isPaused)
            Resume();
    }

    private void Pause()
    {
        isPaused = true;

        // Save current state
        savedVelocity = rb.linearVelocity;
        savedAngularVelocity = rb.angularVelocity;
        savedGravityScale = rb.gravityScale;

        // Freeze
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
        rb.gravityScale = 0f;
        rb.bodyType = RigidbodyType2D.Kinematic;

        // Disable colliders — cannot hit lander
        foreach (var col in cols)
            col.enabled = false;
    }

    private void Resume()
    {
        isPaused = false;

        // Restore physics
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = savedGravityScale;
        rb.linearVelocity = savedVelocity;
        rb.angularVelocity = savedAngularVelocity;

        // Re-enable colliders
        foreach (var col in cols)
            col.enabled = true;
    }
}
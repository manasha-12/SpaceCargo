using UnityEngine;

/// <summary>
/// Add to Asteroid and Drone prefabs.
/// Freezes movement and collisions when game is not active
/// (WaitingToStart, GameOver, or Paused via Time.timeScale).
///
/// Does NOT touch gravityScale — asteroids keep their own gravity settings.
/// Instead freezes velocity and switches to Kinematic to stop physics interactions.
/// Also disables any MonoBehaviour movement scripts on the same GameObject.
/// </summary>
public class EnemyPauser : MonoBehaviour
{
    [Tooltip("Extra MonoBehaviours to disable when paused (e.g. Drone movement script). " +
             "Leave empty to auto-detect.")]
    [SerializeField] private MonoBehaviour[] scriptsToDisable;

    private Rigidbody2D rb;
    private Collider2D[] cols;

    private Vector2 savedVelocity;
    private float savedAngularVelocity;
    private bool isPaused = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        cols = GetComponentsInChildren<Collider2D>();

        // Auto-detect movement scripts if none assigned
        if (scriptsToDisable == null || scriptsToDisable.Length == 0)
        {
            // Get all MonoBehaviours except this one
            var all = GetComponents<MonoBehaviour>();
            var list = new System.Collections.Generic.List<MonoBehaviour>();
            foreach (var mb in all)
                if (mb != this) list.Add(mb);
            scriptsToDisable = list.ToArray();
        }
    }

    private void Start()
    {
        // Pause immediately if game isn't active yet
        if (!GameStateManager.IsGameActive)
            Pause();
    }

    // Use Update (not FixedUpdate) so it runs even when timeScale = 0
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

        if (rb != null)
        {
            // Save velocity so we can restore it on resume
            savedVelocity = rb.linearVelocity;
            savedAngularVelocity = rb.angularVelocity;

            // Kinematic: stops physics interactions completely
            // Do NOT save/restore gravityScale — asteroids manage their own
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.bodyType = RigidbodyType2D.Kinematic;
        }

        // Disable colliders so they can't hit the lander
        foreach (var col in cols)
            if (col != null) col.enabled = false;

        // Disable movement scripts (Drone AI, asteroid movers, etc)
        foreach (var mb in scriptsToDisable)
            if (mb != null) mb.enabled = false;
    }

    private void Resume()
    {
        isPaused = false;

        // Re-enable movement scripts first
        foreach (var mb in scriptsToDisable)
            if (mb != null) mb.enabled = true;

        if (rb != null)
        {
            // Restore to Dynamic with original velocity
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.linearVelocity = savedVelocity;
            rb.angularVelocity = savedAngularVelocity;
            // gravityScale is untouched — asteroid/drone manages its own
        }

        // Re-enable colliders
        foreach (var col in cols)
            if (col != null) col.enabled = true;
    }
}
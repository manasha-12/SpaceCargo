using System;
using System.Globalization;
using UnityEngine;
using UnityEngine.InputSystem;

public class Lander : MonoBehaviour
{
    private const float GRAVITY_NORAML = 0.7f;
    public static Lander Instance { get; private set; }

    public event EventHandler<OnStateChangedEventAgrs> OnStateChange;
    public class OnStateChangedEventAgrs : EventArgs
    {
        public State state;
    }

    [Header("Health System")]
    [SerializeField] private int maxHealth = 3;
    private int currentHealth;

    public event EventHandler<OnHealthChangedEventArgs> OnHealthChanged;
    public class OnHealthChangedEventArgs : EventArgs
    {
        public int currentHealth;
        public int maxHealth;
        public int damage;
    }

    [Header("Shooting System")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float fireRate = 0.3f;

    private float nextFireTime = 0f;

    public event EventHandler OnUpForce;
    public event EventHandler OnRightForce;
    public event EventHandler OnLeftForce;
    public event EventHandler OnBeforeForce;

    public event EventHandler OnCoinPickup;
    public event EventHandler<OnLandedEventArgs> OnLanded;
    public class OnLandedEventArgs : EventArgs
    {
        public LandingType landingType;
        public int score;
        public float dotVector;
        public float landingSpeed;
        public float scoreMultiplier;
    }

    public enum LandingType
    {
        Success,
        WrongLandingArea,
        TooSteepAngle,
        TooFastLanding,
    }

    public enum State
    {
        WaitingToStart,
        Normal,
        GameOver,
    }

    private Rigidbody2D landerRigidbody2D;
    private float fuelAmount;
    private float fuelAmountMax = 10f;

    private State state = State.WaitingToStart;

    private Coroutine slowFallCoroutine;

    private void Awake()
    {
        fuelAmount = fuelAmountMax;
        currentHealth = maxHealth;

        landerRigidbody2D = GetComponent<Rigidbody2D>();
        Instance = this;

        landerRigidbody2D.linearDamping = 0.5f;
        landerRigidbody2D.angularDamping = 2f;
        landerRigidbody2D.interpolation = RigidbodyInterpolation2D.Interpolate;
        landerRigidbody2D.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        // Start fully frozen — Kinematic so nothing moves or triggers collisions
        landerRigidbody2D.bodyType = RigidbodyType2D.Kinematic;
        landerRigidbody2D.gravityScale = 0f;
        landerRigidbody2D.linearVelocity = Vector2.zero;
        landerRigidbody2D.angularVelocity = 0f;
    }

    private void FixedUpdate()
    {
        OnBeforeForce?.Invoke(this, EventArgs.Empty);

        switch (state)
        {
            default:
            case State.WaitingToStart:
                // Accept both keyboard and controller input to start
                if (GameInput.Instance.IsUpActionPressed() ||
                    GameInput.Instance.IsLeftActionPressed() ||
                    GameInput.Instance.IsRightActionPressed() ||
                    Keyboard.current.upArrowKey.isPressed ||
                    Keyboard.current.leftArrowKey.isPressed ||
                    Keyboard.current.rightArrowKey.isPressed)
                {
                    // SetState(Normal) calls UnfreezeRigidbody which restores Dynamic + gravity
                    SetState(State.Normal);
                }
                break;

            case State.Normal:
                if (fuelAmount <= 0f)
                {
                    OutOfFuel();
                    return;
                }

                if (GameInput.Instance.IsUpActionPressed() ||
                    GameInput.Instance.IsLeftActionPressed() ||
                    GameInput.Instance.IsRightActionPressed())
                {
                    ConsumeFuel();
                    landerRigidbody2D.gravityScale = GRAVITY_NORAML;
                }

                if (GameInput.Instance.IsUpActionPressed())
                {
                    float force = 8f;
                    landerRigidbody2D.AddForce(force * transform.up, ForceMode2D.Force);
                    OnUpForce?.Invoke(this, EventArgs.Empty);
                }

                if (GameInput.Instance.IsRightActionPressed())
                {
                    float turnspeed = -3f;
                    landerRigidbody2D.AddTorque(turnspeed, ForceMode2D.Force);
                    OnRightForce?.Invoke(this, EventArgs.Empty);
                }

                if (GameInput.Instance.IsLeftActionPressed())
                {
                    float turnspeed = 3f;
                    landerRigidbody2D.AddTorque(turnspeed, ForceMode2D.Force);
                    OnLeftForce?.Invoke(this, EventArgs.Empty);
                }

                if (state == State.Normal)
                {
                    HandleShooting();
                }
                break;

            case State.GameOver:
                break;
        }

        landerRigidbody2D.angularVelocity =
            Mathf.Clamp(landerRigidbody2D.angularVelocity, -200f, 200f);
    }

    private void OutOfFuel()
    {
        Debug.Log("Out of fuel! Game Over!");

        if (currentHealth > 0)
            TakeDamage(currentHealth);

        SetState(State.GameOver);
        SceneLoader.LoadScene(SceneLoader.Scene.GameOverScene);
    }

    private bool isInvincible = false;

    private System.Collections.IEnumerator InvincibilityFrames()
    {
        isInvincible = true;

        SpriteRenderer spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            for (int i = 0; i < 6; i++)
            {
                spriteRenderer.enabled = false;
                yield return new WaitForSeconds(0.1f);
                spriteRenderer.enabled = true;
                yield return new WaitForSeconds(0.1f);
            }
        }

        isInvincible = false;
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;

        if (currentHealth < 0)
            currentHealth = 0;

        OnHealthChanged?.Invoke(this, new OnHealthChangedEventArgs
        {
            currentHealth = currentHealth,
            maxHealth = maxHealth,
            damage = damage
        });

        Debug.Log($"Lander took {damage} damage! Health: {currentHealth}/{maxHealth}");
    }

    public int GetCurrentHealth() => currentHealth;
    public int GetMaxHealth() => maxHealth;
    public float GetHealthNormalized() => (float)currentHealth / maxHealth;

    private void OnCollisionEnter2D(Collision2D collision2D)
    {
        if (isInvincible) return;

        if (!collision2D.gameObject.TryGetComponent(out LandingPad landingPad))
        {
            TakeDamage(1);

            if (currentHealth <= 0)
            {
                Debug.Log("The lander has been destroyed! (Health = 0)");

                OnLanded?.Invoke(this, new OnLandedEventArgs
                {
                    landingType = LandingType.WrongLandingArea,
                    dotVector = 0f,
                    landingSpeed = 0f,
                    scoreMultiplier = 0,
                    score = 0,
                });

                SetState(State.GameOver);
            }
            else
            {
                Debug.Log($"Crash survived! Health remaining: {currentHealth}/{maxHealth}");

                Vector2 bounceDirection =
                    (transform.position - collision2D.transform.position).normalized;
                landerRigidbody2D.AddForce(bounceDirection * 5f, ForceMode2D.Impulse);

                StartCoroutine(InvincibilityFrames());
            }
            return;
        }

        // ── Hit a landing pad ─────────────────────────────────────────────

        float softLandingVelocityMagnitude = 4f;
        float relativeVelocityMagnitude = collision2D.relativeVelocity.magnitude;

        if (relativeVelocityMagnitude > softLandingVelocityMagnitude)
        {
            TakeDamage(1);

            if (currentHealth <= 0)
            {
                Debug.Log("Landed too hard - Destroyed!");
                OnLanded?.Invoke(this, new OnLandedEventArgs
                {
                    landingType = LandingType.TooFastLanding,
                    dotVector = 0f,
                    landingSpeed = relativeVelocityMagnitude,
                    scoreMultiplier = 0,
                    score = 0,
                });
                SetState(State.GameOver);
            }
            else
            {
                Debug.Log($"Landed too hard but survived! Health: {currentHealth}/{maxHealth}");
                landerRigidbody2D.AddForce(Vector2.up * 3f, ForceMode2D.Impulse);
                StartCoroutine(InvincibilityFrames());
            }
            return;
        }

        float dotVector = Vector2.Dot(Vector2.up, transform.up);
        float minDotVector = 0.90f;

        if (dotVector < minDotVector)
        {
            TakeDamage(1);

            if (currentHealth <= 0)
            {
                Debug.Log("Landed at too steep angle - Destroyed!");
                OnLanded?.Invoke(this, new OnLandedEventArgs
                {
                    landingType = LandingType.TooSteepAngle,
                    dotVector = dotVector,
                    landingSpeed = relativeVelocityMagnitude,
                    scoreMultiplier = 0,
                    score = 0,
                });
                SetState(State.GameOver);
            }
            else
            {
                Debug.Log($"Landed at steep angle but survived! Health: {currentHealth}/{maxHealth}");
                landerRigidbody2D.AddForce(transform.up * 3f, ForceMode2D.Impulse);
                StartCoroutine(InvincibilityFrames());
            }
            return;
        }

        // ── SUCCESSFUL LANDING ────────────────────────────────────────────

        Debug.Log("Successful Landing!");

        landingPad.OnLanderLanded();

        float maxScoreAmountLandingAngle = 100;
        float scoreDotVectorMultiplier = 10f;
        float landingAngleScore = maxScoreAmountLandingAngle
            - Mathf.Abs(dotVector - 1f) * scoreDotVectorMultiplier * maxScoreAmountLandingAngle;

        float maxScoreAmountLandingSpeed = 100;
        float landingSpeedScore = (softLandingVelocityMagnitude - relativeVelocityMagnitude)
            * maxScoreAmountLandingSpeed;

        int score = Mathf.RoundToInt(
            (landingAngleScore + landingSpeedScore) * landingPad.GetScoreMultiplier());

        OnLanded?.Invoke(this, new OnLandedEventArgs
        {
            landingType = LandingType.Success,
            dotVector = dotVector,
            landingSpeed = relativeVelocityMagnitude,
            scoreMultiplier = landingPad.GetScoreMultiplier(),
            score = score,
        });

        SetState(State.GameOver);
    }

    public void ResetHealth()
    {
        currentHealth = maxHealth;

        OnHealthChanged?.Invoke(this, new OnHealthChangedEventArgs
        {
            currentHealth = currentHealth,
            maxHealth = maxHealth,
            damage = 0
        });

        Debug.Log("Health reset to full: " + currentHealth + "/" + maxHealth);
    }

    private void OnTriggerEnter2D(Collider2D collider2D)
    {
        if (collider2D.gameObject.TryGetComponent(out FuelPickUp fuelPickUp))
        {
            float addFuelAmount = fuelPickUp.GetFuelAmount();
            fuelAmount += addFuelAmount;

            if (fuelAmount > fuelAmountMax)
                fuelAmount = fuelAmountMax;

            Debug.Log("Fuel collected! Adding: " + addFuelAmount
                      + " | New fuel total: " + fuelAmount);

            AchievementManager.Instance?.OnFuelPickedUp();

            fuelPickUp.destroySelf();
        }

        if (collider2D.gameObject.TryGetComponent(out CoinPickup coinPickup))
        {
            OnCoinPickup?.Invoke(this, EventArgs.Empty);
            coinPickup.DestroySelf();
        }
    }

    // ── State machine ─────────────────────────────────────────────────────
    private void SetState(State state)
    {
        this.state = state;

        switch (state)
        {
            case State.WaitingToStart:
                FreezeRigidbody();
                GameStateManager.SetGameInactive(); // pause all enemies
                break;

            case State.Normal:
                UnfreezeRigidbody();
                GameStateManager.SetGameActive();   // resume all enemies
                break;

            case State.GameOver:
                FreezeRigidbody();
                GameStateManager.SetGameInactive(); // pause all enemies after landing
                // Disable colliders so nothing can hit the lander
                foreach (var col in GetComponents<Collider2D>())
                    col.enabled = false;
                foreach (var col in GetComponentsInChildren<Collider2D>())
                    col.enabled = false;
                break;
        }

        OnStateChange?.Invoke(this, new OnStateChangedEventAgrs { state = state });
    }

    private void FreezeRigidbody()
    {
        if (landerRigidbody2D == null) return;
        landerRigidbody2D.gravityScale = 0f;
        landerRigidbody2D.linearVelocity = Vector2.zero;
        landerRigidbody2D.angularVelocity = 0f;
        landerRigidbody2D.bodyType = RigidbodyType2D.Kinematic;
    }

    private void UnfreezeRigidbody()
    {
        if (landerRigidbody2D == null) return;
        landerRigidbody2D.bodyType = RigidbodyType2D.Dynamic;
        landerRigidbody2D.gravityScale = GRAVITY_NORAML;
    }

    private void ConsumeFuel()
    {
        float fuelConsumptionValue = 1f;
        fuelAmount -= fuelConsumptionValue * Time.deltaTime;
    }

    public float GetFuel() => fuelAmount;
    public float GetSpeedX() => landerRigidbody2D.linearVelocityX;
    public float GetSpeedY() => landerRigidbody2D.linearVelocityY;
    public float GetFuelAmountNormalized() => fuelAmount / fuelAmountMax;

    public void ApplySlowFall(float duration, float reducedGravity)
    {
        if (slowFallCoroutine != null)
            StopCoroutine(slowFallCoroutine);

        slowFallCoroutine = StartCoroutine(SlowFallCoroutine(duration, reducedGravity));
    }

    private System.Collections.IEnumerator SlowFallCoroutine(float duration, float reducedGravity)
    {
        float originalGravity = landerRigidbody2D.gravityScale;
        landerRigidbody2D.gravityScale = reducedGravity;

        Debug.Log("Slow Fall Active!");
        yield return new WaitForSeconds(duration);

        landerRigidbody2D.gravityScale = originalGravity;
        Debug.Log("Slow Fall Ended!");

        slowFallCoroutine = null;
    }

    private void HandleShooting()
    {
        if (GameInput.Instance.IsShootPressed() && Time.time >= nextFireTime)
        {
            ShootProjectile();
            nextFireTime = Time.time + fireRate;
        }
    }

    private void ShootProjectile()
    {
        if (bulletPrefab == null)
        {
            Debug.LogWarning("Bullet prefab not assigned!");
            return;
        }

        Vector3 spawnPosition = firePoint != null ? firePoint.position : transform.position;
        Instantiate(bulletPrefab, spawnPosition, transform.rotation);

        ParticleSystem muzzleFlash = firePoint?.GetComponentInChildren<ParticleSystem>();
        if (muzzleFlash != null)
            muzzleFlash.Play();

        Debug.Log("Bullet fired!");
    }
}
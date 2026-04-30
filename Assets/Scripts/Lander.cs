using System;
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

    private State state=State.WaitingToStart;

    private Coroutine slowFallCoroutine;

    private void Awake()
    {
        fuelAmount = fuelAmountMax;

        landerRigidbody2D = GetComponent<Rigidbody2D>();
        landerRigidbody2D.gravityScale = 0f;

        Instance = this;

        // Configure rigidbody for smooth movement
        landerRigidbody2D.linearDamping = 0.5f;
        landerRigidbody2D.angularDamping = 2f;
        landerRigidbody2D.interpolation = RigidbodyInterpolation2D.Interpolate;
        landerRigidbody2D.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
    }

    private void FixedUpdate()
    {
        OnBeforeForce?.Invoke(this, EventArgs.Empty);

        switch (state)
        {
            default:
                case State.WaitingToStart:
                    if (Keyboard.current.upArrowKey.isPressed ||
                        Keyboard.current.leftArrowKey.isPressed ||
                        Keyboard.current.rightArrowKey.isPressed)
                    {
                        landerRigidbody2D.gravityScale = GRAVITY_NORAML;
                        SetState(State.Normal);
                    }
                    break;
            case State.Normal:
                if (fuelAmount <= 0f)
                {
                    Debug.Log("Out of fuel!");
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
                break;
            case State.GameOver:
                break;
        }

        

        // Limit angular velocity for smoother rotation
        landerRigidbody2D.angularVelocity = Mathf.Clamp(landerRigidbody2D.angularVelocity, -200f, 200f);
    }

    private void OnCollisionEnter2D(Collision2D collision2D)
    {
        if (!collision2D.gameObject.TryGetComponent(out LandingPad landingPad))
        {
            Debug.Log("The lander has crashed!");
            OnLanded?.Invoke(this, new OnLandedEventArgs
            {
                landingType = LandingType.WrongLandingArea,
                dotVector = 0f,
                landingSpeed = 0f,
                scoreMultiplier = 0,
                score = 0,
            });
            SetState(State.GameOver);
            return;
        }

        float softLandingVelocityMagnitude = 4f;
        float relativeVelocityMagnitude = collision2D.relativeVelocity.magnitude;

        if (relativeVelocityMagnitude > softLandingVelocityMagnitude)
        {
            Debug.Log("Landed too hard!");
            OnLanded?.Invoke(this, new OnLandedEventArgs
            {
                landingType = LandingType.TooFastLanding,
                dotVector = 0f,
                landingSpeed = relativeVelocityMagnitude,
                scoreMultiplier = 0,
                score = 0,
            });
            SetState(State.GameOver);
            return;
        }

        float dotVector = Vector2.Dot(Vector2.up, transform.up);
        float minDotVector = .90f;

        if (dotVector < minDotVector)
        {
            Debug.Log("Landed on a too steep angle");
            OnLanded?.Invoke(this, new OnLandedEventArgs
            {
                landingType = LandingType.TooSteepAngle,
                dotVector = dotVector,
                landingSpeed = relativeVelocityMagnitude,
                scoreMultiplier = 0,
                score = 0,
            });
            SetState(State.GameOver);
            return;
        }

        Debug.Log("Successful Landing!");

        float maxScoreAmountLandingAngle = 100;
        float scoreDotVectorMultiplier = 10f;
        float landingAngleScore = maxScoreAmountLandingAngle - Mathf.Abs(dotVector - 1f) * scoreDotVectorMultiplier * maxScoreAmountLandingAngle;

        float maxScoreAmountLandingSpeed = 100;
        float landingSpeedScore = (softLandingVelocityMagnitude - relativeVelocityMagnitude) * maxScoreAmountLandingSpeed;

        //Debug.Log("landing angle score: " + landingAngleScore);
        //Debug.Log("landing speed score: " + landingSpeedScore);

        int score = Mathf.RoundToInt((landingAngleScore + landingSpeedScore) * landingPad.GetScoreMultiplier());
        //Debug.Log("Score:" + score);
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

    private void OnTriggerEnter2D(Collider2D collider2D)
    {
        if (collider2D.gameObject.TryGetComponent(out FuelPickUp fuelPickUp))
        {
            float addFuelAmount = fuelPickUp.GetFuelAmount();
            fuelAmount += addFuelAmount;

            if (fuelAmount > fuelAmountMax)
            {
                fuelAmount = fuelAmountMax;
            }

            Debug.Log("Fuel collected! Adding: " + addFuelAmount + " | New fuel total: " + fuelAmount);

            fuelPickUp.destroySelf();
        }

        if (collider2D.gameObject.TryGetComponent(out CoinPickup coinPickup))
        {
            OnCoinPickup?.Invoke(this, EventArgs.Empty);

            coinPickup.DestroySelf();
        }
    }

    private void SetState(State state)
    {
        this.state = state;
        OnStateChange?.Invoke(this, new OnStateChangedEventAgrs { state = state});
    }

    private void ConsumeFuel()
    {
        float fuelConsumptionValue = 1f;
        fuelAmount -= fuelConsumptionValue * Time.deltaTime;
    }

    public float GetFuel()
    {
        return fuelAmount;
    }

    public float GetSpeedX()
    {
       return landerRigidbody2D.linearVelocityX;
    }

    public float GetSpeedY()
    {
        return landerRigidbody2D.linearVelocityY;
    }

    public float GetFuelAmountNormalized()
    {
        return fuelAmount / fuelAmountMax;
    }

    public void ApplySlowFall(float duration, float reducedGravity)
    {
        // Stop existing slow fall if active
        if (slowFallCoroutine != null)
        {
            StopCoroutine(slowFallCoroutine);
        }

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
}
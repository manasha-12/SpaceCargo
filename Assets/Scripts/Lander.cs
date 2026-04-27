using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class Lander : MonoBehaviour
{
    private const float GRAVITY_NORAML = 0.7f;
    public static Lander Instance { get; private set; }

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
    }

    private Rigidbody2D landerRigidbody2D;
    private float fuelAmount;
    private float fuelAmountMax = 10f;

    private State state=State.WaitingToStart;

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
                        state = State.Normal;
                    }
                    break;
            case State.Normal:
                if (fuelAmount <= 0f)
                {
                    Debug.Log("Out of fuel!");
                    return;
                }

                if (Keyboard.current.upArrowKey.isPressed ||
                    Keyboard.current.leftArrowKey.isPressed ||
                    Keyboard.current.rightArrowKey.isPressed)
                {
                    ConsumeFuel();
                    landerRigidbody2D.gravityScale = GRAVITY_NORAML;
                }

                if (Keyboard.current.upArrowKey.isPressed)
                {
                    float force = 8f;
                    landerRigidbody2D.AddForce(force * transform.up, ForceMode2D.Force);
                    OnUpForce?.Invoke(this, EventArgs.Empty);
                }

                if (Keyboard.current.rightArrowKey.isPressed)
                {
                    float turnspeed = -3f;
                    landerRigidbody2D.AddTorque(turnspeed, ForceMode2D.Force);
                    OnRightForce?.Invoke(this, EventArgs.Empty);
                }

                if (Keyboard.current.leftArrowKey.isPressed)
                {
                    float turnspeed = 3f;
                    landerRigidbody2D.AddTorque(turnspeed, ForceMode2D.Force);
                    OnLeftForce?.Invoke(this, EventArgs.Empty);
                }
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
}
using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class Lander : MonoBehaviour
{
    public event EventHandler OnUpForce;
    public event EventHandler OnRightForce;
    public event EventHandler OnLeftForce;
    public event EventHandler OnBeforeForce;

    private Rigidbody2D landerRigidbody2D;
    private float fuelAmount = 10f;

    private void Awake()
    {
        landerRigidbody2D = GetComponent<Rigidbody2D>();

        // Configure rigidbody for smooth movement
        landerRigidbody2D.linearDamping = 0.5f;
        landerRigidbody2D.angularDamping = 2f;
        landerRigidbody2D.interpolation = RigidbodyInterpolation2D.Interpolate;
        landerRigidbody2D.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
    }

    private void FixedUpdate()
    {
        OnBeforeForce?.Invoke(this, EventArgs.Empty);
        Debug.Log(fuelAmount);
        if (fuelAmount <= 0f)
        {
            return;
        }

        if (Keyboard.current.upArrowKey.isPressed || 
            Keyboard.current.leftArrowKey.isPressed ||
            Keyboard.current.rightArrowKey.isPressed)
        {
            ConsumeFuel();
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

        // Limit angular velocity for smoother rotation
        landerRigidbody2D.angularVelocity = Mathf.Clamp(landerRigidbody2D.angularVelocity, -200f, 200f);
    }

    private void OnCollisionEnter2D(Collision2D collision2D)
    {
        if (!collision2D.gameObject.TryGetComponent(out LandingPad landingPad))
        {
            Debug.Log("The lander has crashed!");
            return;
        }

        float softLandingVelocityMagnitude = 4f;
        float relativeVelocityMagnitude = collision2D.relativeVelocity.magnitude;

        if (relativeVelocityMagnitude > softLandingVelocityMagnitude)
        {
            // landed to hard!
            Debug.Log("Landed to hard!");
            return;
        }

        float dotVector = Vector2.Dot(Vector2.up, transform.up);
        float minDotVector = .90f;

        if (dotVector < minDotVector)
        {
            // Landed on a too steep angle!
            Debug.Log("Landed on a too steep angle");
            return;
        }

        Debug.Log("Successful Lnading!");

        float maxScoreAmountLandingAngle = 100;
        float scoreDotVectorMultiplier = 10f;
        float landingAngleScore = maxScoreAmountLandingAngle - Mathf.Abs(dotVector - 1f) * scoreDotVectorMultiplier * maxScoreAmountLandingAngle;

        float maxScoreAmountLandingSpped = 100;
        float landingSpeedScore = (softLandingVelocityMagnitude - relativeVelocityMagnitude) * maxScoreAmountLandingSpped;

        Debug.Log("landing angle score: " + landingAngleScore);
        Debug.Log("landing speed score: " + landingSpeedScore);

        int score = Mathf.RoundToInt((landingAngleScore + landingSpeedScore) * landingPad.GetScoreMultiplier());
        Debug.Log("Score:" + score);
    }

    private void OnTriggerEnter2D(Collider2D collider2D)
    {
        //if (collider2D.gameObject.TryGetComponent<>)
    }

    private void ConsumeFuel()
    {
        float fuelConsumptionValue = 1f;
        fuelAmount -= fuelConsumptionValue * Time.deltaTime;
    }
}
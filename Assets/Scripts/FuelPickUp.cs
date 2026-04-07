using UnityEngine;

public class FuelPickUp : MonoBehaviour
{
    [SerializeField] private float fuelAmount = 10f;

    public float GetFuelAmount()
    {
        return fuelAmount;
    }
}
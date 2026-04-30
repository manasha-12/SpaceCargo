// Create: RotateAndGlow.cs
using UnityEngine;

public class RotateAndGlow : MonoBehaviour
{
    [SerializeField] private float rotationSpeed = 100f;
    [SerializeField] private float glowSpeed = 2f;
    [SerializeField] private float glowMin = 0.5f;
    [SerializeField] private float glowMax = 1.5f;

    private SpriteRenderer spriteRenderer;

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        // Rotate
        transform.Rotate(Vector3.forward * rotationSpeed * Time.deltaTime);

        // Pulsing glow
        float scale = Mathf.Lerp(glowMin, glowMax, Mathf.PingPong(Time.time * glowSpeed, 1f));
        transform.localScale = Vector3.one * scale;
    }
}
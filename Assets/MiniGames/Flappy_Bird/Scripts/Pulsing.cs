using UnityEngine;

public class OrganicButtonPulse : MonoBehaviour
{
    [Header("Pulse Settings")]
    [Tooltip("Speed of the organic pulse")]
    public float pulseSpeed = 1.3f;

    [Tooltip("How strong the pulse is")]
    public float pulseAmount = 0.04f;

    [Tooltip("Adds unevenness to the pulse")]
    public float irregularity = 0.5f;

    private Vector3 baseScale;
    private float seed;

    void Awake()
    {
        baseScale = transform.localScale;
        seed = Random.Range(0f, 100f); // prevents identical pulses
    }

    void Update()
    {
        float t = (Time.time + seed) * pulseSpeed;

        // Organic, uneven pulse (feels biological)
        float pulse =
            1f +
            Mathf.Max(0f, Mathf.Sin(t)) * pulseAmount +
            Mathf.Sin(t * irregularity) * (pulseAmount * 0.3f);

        transform.localScale = baseScale * pulse;
    }
}

using UnityEngine;

public class PulseEffect : MonoBehaviour
{
    public float pulseSpeed = 2f;
    public float minScale = 0.95f;
    public float maxScale = 1.05f;

    private Vector3 baseScale;

    private void Start()
    {
        baseScale = transform.localScale;
    }

    private void Update()
    {
        // Creates a smooth "breathing" motion using Sine wave
        float scaleOffset = Mathf.PingPong(Time.time * pulseSpeed, maxScale - minScale) + minScale;
        
        // Apply the scale evenly
        transform.localScale = baseScale * scaleOffset;
        
        // Optional: Slightly pulse the red color intensity (alpha)
        // SpriteRenderer sr = GetComponent<SpriteRenderer>();
        // Color color = sr.color;
        // color.a = scaleOffset; 
        // sr.color = color;
    }
}
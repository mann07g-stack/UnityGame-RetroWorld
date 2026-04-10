using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public float smoothTime = 0.25f;
    public float bounceStrength = 0.15f;

    private Vector3 velocity = Vector3.zero;
    private Vector3 offset;

    void Start()
    {
        offset = transform.position - target.position;
    }

    void LateUpdate()
    {
        Vector3 targetPos = target.position + offset;

        // small vertical bounce
        float bounce = Mathf.Sin(Time.time * 8f) * bounceStrength;
        targetPos.y += bounce;

        transform.position = Vector3.SmoothDamp(
            transform.position,
            targetPos,
            ref velocity,
            smoothTime
        );
    }
}

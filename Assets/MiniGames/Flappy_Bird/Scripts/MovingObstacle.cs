using UnityEngine;

public class MovingObstacle : MonoBehaviour
{
    public enum MovementType { VerticalPingPong, Rotate }
    public MovementType movementType;

    [Header("Vertical Settings")]
    public float moveSpeed = 1f;
    public float moveRange = 1f;

    [Header("Rotation Settings")]
    public float rotationSpeed = 90f;

    private Vector3 startLocalPos;

    private void Start()
    {
        startLocalPos = transform.localPosition;
    }

    private void Update()
    {
        if (movementType == MovementType.VerticalPingPong)
        {
            // Moves the object up and down continuously
            float newY = startLocalPos.y + Mathf.PingPong(Time.time * moveSpeed, moveRange) - (moveRange / 2f);
            transform.localPosition = new Vector3(startLocalPos.x, newY, startLocalPos.z);
        }
        else if (movementType == MovementType.Rotate)
        {
            // Rotates the object
            transform.Rotate(Vector3.forward * rotationSpeed * Time.deltaTime);
        }
    }
}
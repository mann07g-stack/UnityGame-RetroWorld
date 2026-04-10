using UnityEngine;

public class ObstacleMover : MonoBehaviour
{
    public float speed = 2f;             // movement speed
    public float distance = 4f;          // total movement range

    private Vector3 startPosition;

    void Start()
    {
        startPosition = transform.position;
    }

    void Update()
    {
        float offset = Mathf.PingPong(Time.time * speed, distance);
        transform.position = startPosition + Vector3.right * (offset - distance / 2f);
    }
}

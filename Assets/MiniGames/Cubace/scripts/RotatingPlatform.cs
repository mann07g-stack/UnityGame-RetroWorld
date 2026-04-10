using UnityEngine;

public class RotatingPlatform : MonoBehaviour
{
    public Vector3 rotationSpeed = new Vector3(0f, 50f, 0f); // Rotation in degrees per second

    void Update()
    {
        transform.Rotate(rotationSpeed * Time.deltaTime*5);
    }
}

using UnityEngine;

public class GeomCameraFollow : MonoBehaviour
{
    public Transform target;
    public float xOffset = 3f;
    public float yOffset = 0f;

    void LateUpdate()
    {
        if (target == null) return; // If there's no target, do nothing

        transform.position = new Vector3(
            target.position.x + xOffset,
            target.position.y + yOffset,
            -10f
        );
    }
}
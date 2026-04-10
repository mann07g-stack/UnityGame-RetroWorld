using UnityEngine;

public class FollowPlayerX : MonoBehaviour
{
    public Transform player;
    public float followStrength = 1f; // 1 = exact follow, <1 = parallax

    float startX;

    void Start()
    {
        startX = transform.position.x;
    }

    void LateUpdate()
    {
        float targetX = Mathf.Lerp(
            startX,
            player.position.x,
            followStrength
        );

        transform.position = new Vector3(
            targetX,
            transform.position.y,
            transform.position.z
        );
    }
}

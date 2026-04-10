using UnityEngine;

public class BackgroundScroll : MonoBehaviour
{
    [Header("References")]
    public Transform player;

    [Header("Parallax")]
    public float horizontalScrollSpeed = -0.25f;
    public float verticalFollowFactor = 0.3f; // 0 = fixed, 1 = full follow

    Material mat;
    Vector2 offset;

    float startY;

    void Start()
    {
        mat = GetComponent<SpriteRenderer>().material;
        offset = mat.mainTextureOffset;
        startY = transform.position.y;
    }

    void Update()
    {
        // Horizontal texture scroll (parallax)
        offset.x += horizontalScrollSpeed * Time.deltaTime;
        offset.y = 0f; // 🔒 never scroll texture vertically
        mat.mainTextureOffset = offset;

        // Vertical parallax follow (world space)
        float targetY = Mathf.Lerp(
            startY,
            player.position.y,
            verticalFollowFactor
        );

        transform.position = new Vector3(
            transform.position.x,
            targetY,
            transform.position.z
        );
    }
}

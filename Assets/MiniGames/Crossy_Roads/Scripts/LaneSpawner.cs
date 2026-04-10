using UnityEngine;

public class LaneSpawner : MonoBehaviour
{
    public GameObject smallObstaclePrefab; // 1 x 1.5
    public GameObject longObstaclePrefab;  // 1 x 3

    public float laneSpeed = 4f;
    public float wrapX = 50f;

    public int maxObstacles = 25;

    [Header("Gap Control")]
    public float maxGap = 7f; // YOU TYPE THIS (min is always 1)

    public bool moveRight = true;

    void Start()
    {
        float cursorX = -wrapX;

        for (int i = 0; i < maxObstacles; i++)
        {
            // ---- 60 / 40 size split ----
            GameObject prefab = (Random.value < 0.6f)
                ? smallObstaclePrefab
                : longObstaclePrefab;

            float obstacleWidth = prefab == longObstaclePrefab ? 3f : 1.5f;

            // ---- TRUE GAP (empty space only) ----
            float gap = Random.Range(1f, maxGap);

            // Move cursor by half obstacle + gap + half obstacle
            cursorX += obstacleWidth * 0.5f;
            cursorX += gap;
            cursorX += obstacleWidth * 0.5f;

            if (cursorX > wrapX)
                break;

            Spawn(prefab, cursorX);
        }
    }

    void Spawn(GameObject prefab, float x)
    {
        GameObject obj = Instantiate(
            prefab,
            new Vector3(x, transform.position.y, 0),
            Quaternion.identity,
            transform
        );

        Obstacle o = obj.GetComponent<Obstacle>();
        o.speed = moveRight ? laneSpeed : -laneSpeed;
        o.wrapX = wrapX;
    }
}

using UnityEngine;

public class Obstacle : MonoBehaviour
{
    public float speed;
    public float wrapX;

    private float halfWidth;

    void Awake()
    {
        halfWidth = transform.localScale.x * 0.5f;
    }

    void Update()
    {
        Vector3 pos = transform.position;
        pos.x += speed * Time.deltaTime;

        if (pos.x > wrapX + halfWidth)
            pos.x = -wrapX - halfWidth;
        else if (pos.x < -wrapX - halfWidth)
            pos.x = wrapX + halfWidth;

        transform.position = pos;
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        if (col.collider.CompareTag("Player"))
        {
            // LOOK FOR THE NEW NAME HERE:
            CrossyPlayerController pc = col.collider.GetComponent<CrossyPlayerController>();
            if (pc != null)
            {
                pc.Die();
            }
        }
    }
}
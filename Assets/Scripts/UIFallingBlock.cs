using UnityEngine;

public class UIFallingBlock : MonoBehaviour
{
    public float fallSpeed = 900f;

    private RectTransform rect;
    private float targetY;
    private bool landed;

    void Awake()
    {
        rect = GetComponent<RectTransform>();
    }

    public void Init(float y)
    {
        targetY = y;
        landed = false;
    }

    void Update()
    {
        if (landed) return;

        rect.anchoredPosition += Vector2.down * fallSpeed * Time.deltaTime;

        if (rect.anchoredPosition.y <= targetY)
        {
            rect.anchoredPosition =
                new Vector2(rect.anchoredPosition.x, targetY);
            landed = true;
        }
    }
}

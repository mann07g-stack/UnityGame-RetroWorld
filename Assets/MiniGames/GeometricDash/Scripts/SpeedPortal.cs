using UnityEngine;

public class SpeedPortal : MonoBehaviour
{
    public enum SpeedType
    {
        Slow,
        Normal,
        Fast
    }

    public SpeedType speedType;

    void OnTriggerEnter2D(Collider2D other)
    {
        GeomPlayerController p = other.GetComponent<GeomPlayerController>();
        if (p == null) return;

        switch (speedType)
        {
            case SpeedType.Slow:
                p.SetSpeed(p.slowSpeed);
                break;

            case SpeedType.Normal:
                p.SetSpeed(p.normalSpeed);
                break;

            case SpeedType.Fast:
                p.SetSpeed(p.fastSpeed);
                break;
        }
    }
}

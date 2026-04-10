using UnityEngine;

public class YellowOrb : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D other)
    {
        GeomPlayerController p = other.GetComponent<GeomPlayerController>();
        if (p != null)
            p.SetOrbOverlap(true, false);
    }

    void OnTriggerExit2D(Collider2D other)
    {
        GeomPlayerController p = other.GetComponent<GeomPlayerController>();
        if (p != null)
            p.SetOrbOverlap(false, false);
    }
}

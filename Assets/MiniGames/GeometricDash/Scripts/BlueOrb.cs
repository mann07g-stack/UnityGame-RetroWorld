using UnityEngine;

public class BlueOrb : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D other)
    {
        GeomPlayerController p = other.GetComponent<GeomPlayerController>();
        if (p != null)
            p.SetOrbOverlap(true, true);
    }

    void OnTriggerExit2D(Collider2D other)
    {
        GeomPlayerController p = other.GetComponent<GeomPlayerController>();
        if (p != null)
            p.SetOrbOverlap(false, true);
    }
}

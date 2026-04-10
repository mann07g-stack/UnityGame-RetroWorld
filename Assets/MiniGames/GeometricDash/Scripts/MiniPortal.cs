using UnityEngine;

public class MiniPortal : MonoBehaviour
{
    public bool makeMini = true;
    bool used = false;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (used) return;

        GeomPlayerController p = other.GetComponent<GeomPlayerController>();
        if (p != null)
        {
            p.ToggleMini(makeMini);
            used = true;
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.GetComponent<GeomPlayerController>() != null)
        {
            used = false;
        }
    }
}

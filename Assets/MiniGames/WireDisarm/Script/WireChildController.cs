using UnityEngine;
using UnityEngine.UI;

public class WireChildController : MonoBehaviour
{
    public Image wireImage;

    void Awake()
    {
        if (wireImage == null) wireImage = GetComponent<Image>();
    }

    public void SetColor(Color color)
    {
        if (wireImage != null)
        {
            wireImage.color = color;
        }
    }

    public void CutWire()
    {
        // Dim the wire to show it's cut
        if (wireImage != null)
        {
            Color c = wireImage.color;
            c.a = 0.3f; // Make transparent
            wireImage.color = c;
        }
    }
}
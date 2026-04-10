using UnityEngine;
using UnityEngine.UI;

public class Lightsout_LightNode : MonoBehaviour
{
    public int x;
    public int y;
    public bool isOn = false;

    [Header("Visuals")]
    public Sprite onSprite;
    public Sprite offSprite;

    private Image img;
    private Lightsout_GridManager manager;

    public void Init(int xPos, int yPos, Lightsout_GridManager m)
    {
        x = xPos;
        y = yPos;
        manager = m;
        img = GetComponent<Image>();
        UpdateVisuals();
    }

    public void Toggle()
    {
        isOn = !isOn;
        UpdateVisuals();
    }

    void UpdateVisuals()
    {
        if (img == null) return;

        img.sprite = isOn ? onSprite : offSprite;
        // Optional: Change color tint if sprites are white
        img.color = Color.white; 
    }

    public void OnClick()
    {
        if (manager != null)
        {
            manager.OnNodeClicked(x, y);
        }
    }
}
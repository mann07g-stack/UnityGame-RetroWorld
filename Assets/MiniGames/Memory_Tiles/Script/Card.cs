using UnityEngine;
using UnityEngine.UI;

public class Card : MonoBehaviour
{
    [SerializeField] private Image iconImage;

    public Sprite hiddenIconSprite; // Assign the "Card Back" image in Inspector
    public Sprite iconSprite;       // Assigned by Controller
    public bool isSelected;
    public CardController controller;

    public void OnCardClick()
    {
        if(controller != null)
        {
            controller.SetSelected(this);
        }
    }

    public void SetIconSprite(Sprite sp)
    {
        iconSprite = sp;
    }
    
    public void Show()
    {
        // Simple sprite swap
        iconImage.sprite = iconSprite;
        isSelected = true;
    }

    public void Hide()
    {
        // Swap back to hidden state
        iconImage.sprite = hiddenIconSprite;
        isSelected = false;
    }
}
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasGroup))]
public class RowController : MonoBehaviour
{
    public Image[] slotImages; // Drag the 5 guess buttons here
    public Image[] pegImages;  // Drag the 5 feedback circles here
    private int[] guessData = {-1, -1, -1, -1, -1};

    public void SetSlotColor(int slotIndex, int colorIndex, Color col)
    {
        guessData[slotIndex] = colorIndex;
        slotImages[slotIndex].color = col;
    }

    public int[] GetGuessData() => guessData;

    public void SetPegs(int black, int white)
    {
        for (int i = 0; i < pegImages.Length; i++) {
            if (i < black) pegImages[i].color = Color.red; // Black/Correct
            else if (i < black + white) pegImages[i].color = Color.white; // White/Wrong pos
            else pegImages[i].color = new Color(0, 0, 0, 0); // Hide
        }
    }
}
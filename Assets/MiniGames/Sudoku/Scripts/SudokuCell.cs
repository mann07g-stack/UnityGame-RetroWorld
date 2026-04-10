using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SudokuCell : MonoBehaviour
{
    public int row, col;
    public bool isLocked;

    private int value;
    private Image image;
    private TMP_Text text;
    private SudokuGridManager manager;

    private Color baseTextColor;

    public void Init(int r, int c, SudokuGridManager mgr)
    {
        row = r;
        col = c;
        manager = mgr;

        image = GetComponent<Image>();
        text = GetComponentInChildren<TMP_Text>();

        baseTextColor = text.color;   // store prefab color
        text.text = "";
    }

    public void OnCellClicked()
    {
        if (isLocked) return;
        manager.SelectCell(this);
    }

    public void SetSelected(bool selected)
    {
        // Prefab handles selection visuals via Animation/Color usually
        // Or you can add image.color logic here if needed
        if(image != null)
        {
            // Example visual feedback (optional)
            image.color = selected ? new Color(0.8f, 0.9f, 1f) : Color.white;
        }
    }

    public void SetValue(int newValue)
    {
        if (isLocked) return;

        value = newValue;

        if (value == 0)
        {
            text.text = "";
        }
        else
        {
            text.text = value.ToString();
            text.color = LightenColor(baseTextColor, 0.35f);
        }

        // Only check for win if the grid is valid so far
        if (manager.IsPuzzleSolved())
        {
            manager.OnPuzzleSolved();
        }
    }

    public int GetValue() => value;

    public void LockCell(int val)
    {
        value = val;
        text.text = val.ToString();
        isLocked = true;

        text.color = DarkenColor(baseTextColor, 0.4f);
    }

    private Color LightenColor(Color original, float amount)
    {
        return Color.Lerp(original, Color.white, amount);
    }

    private Color DarkenColor(Color original, float amount)
    {
        return Color.Lerp(original, Color.black, amount);
    }
}
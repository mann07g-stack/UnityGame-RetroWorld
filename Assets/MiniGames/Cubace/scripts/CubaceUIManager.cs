using UnityEngine;

public class CubaceUIHandler : MonoBehaviour
{
    public void OnInfoClicked()
    {
        if (CubaceGameManager.Instance != null)
            CubaceGameManager.Instance.ToggleInfoPanel();
    }
    
    // Assign this to the "Close" button inside your Info Panel
    public void OnCloseInfoClicked()
    {
        if (CubaceGameManager.Instance != null)
            CubaceGameManager.Instance.ToggleInfoPanel();
    }
}
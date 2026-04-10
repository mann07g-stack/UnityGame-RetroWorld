using UnityEngine;

public class InfoPanelManager : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject infoPanel; // The Panel containing the Book and Text
    public GameObject openButton; // The 'i' button (optional to hide it when open)
    private bool isPaused = false;

    void Start()
    {
        // Ensure the panel is hidden when the game starts
        if (infoPanel != null)
            infoPanel.SetActive(false);
    }

    public void OpenInfoPanel()
    {
        isPaused = true;
        
        // 1. Show the panel
        infoPanel.SetActive(true);
        
        // 2. Hide the open button (optional, keeps UI clean)
        if (openButton != null) openButton.SetActive(false);

        // 3. PAUSE THE GAME
        // Setting timeScale to 0 stops Time.deltaTime, 
        // which automatically freezes your TimerManager and Player Movement.
        Time.timeScale = 0f;
    }

    public void CloseInfoPanel()
    {
        isPaused = false;

        // 1. Hide the panel
        infoPanel.SetActive(false);

        // 2. Show the open button again
        if (openButton != null) openButton.SetActive(true);

        // 3. RESUME THE GAME
        Time.timeScale = 1f;
    }
}
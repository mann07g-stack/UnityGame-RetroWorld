using UnityEngine;

public class InfoMenuController : MonoBehaviour
{
    [Header("UI References")]
    public GameObject infoPanel;
    public GameObject infoButton;
    public MusicController musicController;


    [Header("Timer")]
    public GeomGameTimer gameTimer; // <-- drag GameTimer here

    bool isOpen = false;

    void Start()
    {
        infoPanel.SetActive(false);
        infoButton.SetActive(true);
    }

    public void OpenInfo()
    {
        if (isOpen) return;

        isOpen = true;

        infoPanel.SetActive(true);
        infoButton.SetActive(false);

        Time.timeScale = 0f;

        if (gameTimer != null)
            gameTimer.PauseTimer();

        if (musicController != null)
            musicController.PauseMusic();
    }

    public void CloseInfo()
    {
        if (!isOpen) return;

        isOpen = false;

        infoPanel.SetActive(false);
        infoButton.SetActive(true);

        Time.timeScale = 1f;

        if (gameTimer != null)
            gameTimer.ResumeTimer();

        if (musicController != null)
            musicController.ResumeMusic();
    }
}
using UnityEngine;
using TMPro;

public class MineInfoPanel : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI infoText;
    public GameObject prevButton;
    public GameObject nextButton;

    [Header("Pages")]
    [TextArea(5, 10)]
    public string[] pages;

    [Header("Timer")]
    public MineTimeManager timerManager;

    private int currentPage = 0;

    // 🔹 CALLED BY INFO BUTTON
    public void OpenPanel()
    {
        gameObject.SetActive(true);
    }

    void OnEnable()
    {
        currentPage = 0;
        RefreshText();

        if (timerManager != null)
            timerManager.PauseTimer();
    }

    void OnDisable()
    {
        if (timerManager != null)
            timerManager.ResumeTimer();
    }

    public void ClosePanel()
    {
        gameObject.SetActive(false);
    }

    public void NextPage()
    {
        if (currentPage < pages.Length - 1)
        {
            currentPage++;
            RefreshText();
        }
    }

    public void PreviousPage()
    {
        if (currentPage > 0)
        {
            currentPage--;
            RefreshText();
        }
    }

    void RefreshText()
    {
        if (pages == null || pages.Length == 0) return;

        infoText.text = pages[currentPage];

        if (prevButton != null)
            prevButton.SetActive(currentPage > 0);

        if (nextButton != null)
            nextButton.SetActive(currentPage < pages.Length - 1);
    }
}

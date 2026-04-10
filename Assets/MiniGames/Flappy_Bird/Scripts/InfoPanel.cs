using UnityEngine;

public class InfoPanelController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject infoPanel;

    private bool isOpen = false;

    void Awake()
    {
        if (infoPanel != null)
            infoPanel.SetActive(false);
    }

    public void OpenInfoPanel()
    {
        if (isOpen) return;

        isOpen = true;
        infoPanel.SetActive(true);
    }

    public void CloseInfoPanel()
    {
        if (!isOpen) return;

        isOpen = false;
        infoPanel.SetActive(false);
    }
}

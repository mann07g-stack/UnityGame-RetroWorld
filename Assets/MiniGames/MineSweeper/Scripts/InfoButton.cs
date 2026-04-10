using UnityEngine;

public class InfoButton : MonoBehaviour
{
    public GameObject infoPanel;

    public void OpenInfoPanel()
    {
        infoPanel.SetActive(true);
    }
}

using UnityEngine;

public class MazeUIHandler : MonoBehaviour
{
    [Header("UI References")]
    public GameObject infoPanel;
    
    [Header("Player References")]
    public GameObject playerObject; 

    private bool isPanelOpen = false;
    private PlayerLook mouseLookScript;

    void Start()
    {
        // 1. Find the MouseLook script (usually on the Main Camera child)
        if (playerObject != null)
        {
            mouseLookScript = playerObject.GetComponentInChildren<PlayerLook>();
        }

        // 2. Ensure Panel is closed at start
        if(infoPanel != null) infoPanel.SetActive(false);
        
        // 3. Lock Cursor for gameplay
        SetCursorState(true);
    }

    void Update()
    {
        // Legacy Input Check for "I" or "Tab"
        if (Input.GetKeyDown(KeyCode.I) || Input.GetKeyDown(KeyCode.Tab))
        {
            ToggleInfoPanel();
        }
    }

    public void ToggleInfoPanel()
    {
        isPanelOpen = !isPanelOpen;

        if (isPanelOpen)
        {
            OpenPanel();
        }
        else
        {
            ClosePanel();
        }
    }

    void OpenPanel()
    {
        if(infoPanel != null) infoPanel.SetActive(true);
        SetCursorState(false);
        Time.timeScale = 0f;

        // Disable Mouse Look
        if (mouseLookScript != null) mouseLookScript.enabled = false;
    }

    public void ClosePanel()
    {
        if(infoPanel != null) infoPanel.SetActive(false);
        SetCursorState(true);
        Time.timeScale = 1f;

        // Re-enable Mouse Look
        if (mouseLookScript != null) mouseLookScript.enabled = true;
        
        isPanelOpen = false;
    }

    void SetCursorState(bool locked)
    {
        if (locked)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
}
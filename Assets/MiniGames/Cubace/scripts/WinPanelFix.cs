using UnityEngine;

public class WinPanelFix : MonoBehaviour
{
    // This function exists only to catch the Animation Event
    // so Unity doesn't show an error.
    public void NextLevel()
    {
        Debug.Log("🎨 Animation Event 'NextLevel' ignored. Waiting for Manager's 5s timer...");
    }
}
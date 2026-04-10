using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class MiniGameSession : MonoBehaviour
{
    [Header("Settings")]
    public string npcSceneName = "Test_NPC_Win"; // Make sure this matches your Scene name exactly!

    public void ReportWin()
    {
        StartCoroutine(WinSequence());
    }

    IEnumerator WinSequence()
    {
        Debug.Log("🏆 Game Won! Starting 5-second countdown...");

        // 1. Call API (Unlock Level) - Optional check
        bool apiComplete = false;
        if (APIManager.Instance != null)
        {
            APIManager.Instance.UnlockNextLevel((success) => { apiComplete = true; });
        }
        else
        {
            apiComplete = true; 
        }

        // 2. Wait 5 Real-Time Seconds
        // We use Realtime because the game is paused!
        yield return new WaitForSecondsRealtime(5f);

        // 3. Wait for API if needed
        yield return new WaitUntil(() => apiComplete);

        Debug.Log($"🚀 Returning to {npcSceneName}...");

        // --- CRITICAL FIX: UNFREEZE TIME ---
        // If we don't do this, the next scene will load 'Frozen' and nothing will work.
        Time.timeScale = 1f; 
        // -----------------------------------

        // 4. Load the Scene
        
        Debug.Log($"🚀 Loading Win Scene: {npcSceneName}");
        SceneManager.LoadScene(npcSceneName);
    }
}
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class CubaceGameManager : MonoBehaviour
{
    public static CubaceGameManager Instance;

    [Header("UI References")]
    public GameObject winPanel;
    public GameObject losePanel;    
    public GameObject loadingSpinner;
    public GameObject infoPanel;    

    [Header("Scene Settings")]
    public string backendSceneName = "Test_NPC"; 
    public float restartDelay = 3f;  // Keep 3s for death (so they see "You Died")
    
    // REMOVED: winDelay variable (No longer waiting)

    [Header("Game State")]
    public bool gameHasEnded = false;
    public bool isPaused = false;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        Time.timeScale = 1f; 

        if (winPanel) winPanel.SetActive(false);
        if (losePanel) losePanel.SetActive(false);
        if (loadingSpinner) loadingSpinner.SetActive(false);
        if (infoPanel) infoPanel.SetActive(false);
    }

    public void ToggleInfoPanel()
    {
        if (gameHasEnded) return;

        isPaused = !isPaused;

        if (infoPanel) infoPanel.SetActive(isPaused);
        Time.timeScale = isPaused ? 0f : 1f;
    }

    public void EndGame()
    {
        if (gameHasEnded) return;
        gameHasEnded = true;

        Debug.Log("❌ Game Over.");
        DisablePlayer();

        if (losePanel) losePanel.SetActive(true);
        Invoke("RestartLevel", restartDelay);
    }

    void RestartLevel()
    {
        Time.timeScale = 1f; 
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void CompleteLevel()
    {
        if (gameHasEnded) return;
        gameHasEnded = true;

        Debug.Log("🏆 Level Complete! Starting Backend immediately...");

        DisablePlayer();
        
        // Hide other panels
        if (losePanel) losePanel.SetActive(false);
        if (infoPanel) infoPanel.SetActive(false);

        // Show Win UI briefly
        if (winPanel) winPanel.SetActive(true);

        // START BACKEND IMMEDIATELY (No Wait)
        StartCoroutine(WinSequence());
    }

    void DisablePlayer()
    {
        PlayerMovementSingle player = FindFirstObjectByType<PlayerMovementSingle>();
        if (player != null)
        {
            player.enabled = false; 
            if (player.rb != null) 
            {
                player.rb.linearVelocity = Vector3.zero;
                player.rb.angularVelocity = Vector3.zero;
                player.rb.isKinematic = true; 
            }
        }
    }

    IEnumerator WinSequence()
    {
        // --- 1. REMOVED THE 5-SECOND WAIT ---
        // Proceeding directly to backend logic.

        if (loadingSpinner) loadingSpinner.SetActive(true);

        bool apiSuccess = false;

        // --- 2. CALL API ---
        if (APIManager.Instance != null)
        {
            APIManager.Instance.UnlockNextLevel((success) => 
            {
                apiSuccess = success;
            });
        }
        else
        {
            apiSuccess = true; 
        }

        // --- 3. WAIT FOR API ONLY (Fast as possible) ---
        float timeout = 5f;
        while (timeout > 0 && (loadingSpinner != null && loadingSpinner.activeSelf))
        {
            if (APIManager.Instance == null || apiSuccess) break;
            timeout -= Time.deltaTime;
            yield return null;
        }

        // --- 4. RETURN TO BACKEND ---
        GlobalGameState.isReturningFromGame = true;
        if (GlobalGameState.activeGameData != null)
        {
            GlobalGameState.completedGames.Add(GlobalGameState.activeGameData.gameName);
        }

        Debug.Log($"🔙 Returning to Backend: {backendSceneName}");
        
        Time.timeScale = 1f; 
        SceneManager.LoadScene(backendSceneName);
    }
}
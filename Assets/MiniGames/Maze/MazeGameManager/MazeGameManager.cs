using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class MazeGameManager : MonoBehaviour
{
    public static MazeGameManager Instance;

    [Header("Game Components")]
    // Reference to the player so we can disable movement on win
    public GameObject playerObject; 
    public UITimer gameTimer; 
    public string winSceneName = "Test_NPC";

    [Header("UI References")]
    public GameObject winPanel;
    public GameObject loadingSpinner;

    [Header("Scene Settings")]
    public string defaultReturnScene = "OCity";

    private bool isGameActive = true;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        if (winPanel != null) winPanel.SetActive(false);
        if (loadingSpinner != null) loadingSpinner.SetActive(false);
        
        // Ensure Timer starts
        if (gameTimer != null) gameTimer.StartTimer();
    }

    // Called by MazeWinTrigger when player hits the wall
    public void WinGame()
    {
        if (!isGameActive) return;
        isGameActive = false;

        Debug.Log("🏆 Maze Completed!");

        // 1. Stop Timer
        if (gameTimer != null) gameTimer.StopTimer();

        // 2. Disable Player Controls (Stop movement & Looking)
        if (playerObject != null)
        {
            // Disable Movement
            var movement = playerObject.GetComponent<PlayerMovement>();
            if (movement != null) movement.enabled = false;

            // Disable Mouse Look (Camera rotation)
            // usually on the Camera, which is a child of the player
            var mouseLooks = playerObject.GetComponentsInChildren<PlayerLook>();
            foreach(var ml in mouseLooks) ml.enabled = false;

            // Unlock cursor so they can click buttons
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        // 3. Show UI
        if (winPanel != null) winPanel.SetActive(true);

        // 4. Start Backend Process
        StartCoroutine(WinSequence());
    }

    IEnumerator WinSequence()
    {
        // --- 1. WAIT 5 SECONDS ---
        yield return new WaitForSeconds(5f);

        if (loadingSpinner != null) loadingSpinner.SetActive(true);

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
            apiSuccess = true; // Fallback for testing without API
        }

        // --- 3. WAIT FOR API RESPONSE ---
        float timeout = 5f;
        while (timeout > 0 && (loadingSpinner != null && loadingSpinner.activeSelf))
        {
            if (APIManager.Instance == null || apiSuccess) break;
            timeout -= Time.deltaTime;
            yield return null;
        }

        // --- 4. UPDATE GLOBAL STATE ---
        GlobalGameState.isReturningFromGame = true;
        if (GlobalGameState.activeGameData != null)
        {
            GlobalGameState.completedGames.Add(GlobalGameState.activeGameData.gameName);
        }

        SceneManager.LoadScene(winSceneName);
    }
}
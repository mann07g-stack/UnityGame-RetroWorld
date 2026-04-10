using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class GeomGameManager : MonoBehaviour
{
    public static GeomGameManager Instance;

    [Header("Audio")]
    public MusicController musicController;
    public AudioSource deathAudio;
    public AudioSource winAudio; // Drag a win sound here (optional)

    [Header("UI")]
    public GameObject winPanel;
    public GameObject loadingSpinner; // Drag your Loading Spinner here

    [Header("Scene Settings")]
    public string winSceneName = "Test_NPC"; // Scene to load on win

    [HideInInspector] public bool isDead = false;
    [HideInInspector] public bool isWon = false;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    void Start()
    {
        isDead = false;
        isWon = false;
        
        // Reset Timer
        GeomGameTimer.ResetTimer();

        // Ensure UI is hidden
        if (winPanel) winPanel.SetActive(false);
        if (loadingSpinner) loadingSpinner.SetActive(false);
    }

    // 🔴 CALLED ON DEATH
    public void PlayerDied()
    {
        if (isDead || isWon) return;
        isDead = true;

        if (musicController != null) musicController.StopImmediate();
        if (deathAudio != null) deathAudio.Play();
    }

    // 🔁 RESTART LEVEL
    public void RestartLevel(float delay)
    {
        StartCoroutine(RestartRoutine(delay));
    }

    IEnumerator RestartRoutine(float delay)
    {
        yield return new WaitForSeconds(delay);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    // 🏆 CALLED BY FINISH LINE
    public void LevelComplete()
    {
        if (isDead || isWon) return;
        isWon = true;

        Debug.Log("🏆 Level Complete!");

        // 1. Stop Timer & Audio
        GeomGameTimer.isLevelComplete = true;
        if (musicController != null) musicController.StopImmediate();
        if (winAudio != null) winAudio.Play();

        // 2. Show Win UI
        if (winPanel != null) winPanel.SetActive(true);

        // 3. Freeze Player Physics
        GeomPlayerController player = FindFirstObjectByType<GeomPlayerController>();
        if (player != null)
        {
            player.enabled = false;
            Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
            if (rb) rb.simulated = false; 
        }

        // 4. Call Backend
        StartCoroutine(WinSequence());
    }

    IEnumerator WinSequence()
    {
        if (loadingSpinner) loadingSpinner.SetActive(true);

        bool apiSuccess = false;

        // --- API CALL ---
        if (APIManager.Instance != null)
        {
            APIManager.Instance.UnlockNextLevel((success) => 
            {
                apiSuccess = success;
                if(success) Debug.Log("✅ Backend Confirmed!");
                else Debug.LogError("❌ Backend Failed.");
            });
        }
        else
        {
            apiSuccess = true; // Fallback
        }

        // Wait for API (Max 5s timeout)
        float timeout = 5f;
        while (timeout > 0 && (loadingSpinner != null && loadingSpinner.activeSelf))
        {
            if (APIManager.Instance == null || apiSuccess) break;
            timeout -= Time.deltaTime;
            yield return null;
        }

        // --- UPDATE GLOBAL STATE ---
        GlobalGameState.isReturningFromGame = true;
        if (GlobalGameState.activeGameData != null)
        {
            GlobalGameState.completedGames.Add(GlobalGameState.activeGameData.gameName);
        }

        // Wait a moment for effect
        if (loadingSpinner) loadingSpinner.SetActive(false); // Hide spinner so they see "You Win" clearly
        yield return new WaitForSeconds(5.0f);

        // --- LOAD NPC SCENE ---
        SceneManager.LoadScene(winSceneName);
    }
}
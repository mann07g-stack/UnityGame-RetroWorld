using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement; 
using System.Collections; 

[DefaultExecutionOrder(-1)]
public class FlappyGameManager : MonoBehaviour
{
    public static FlappyGameManager Instance { get; private set; }

    [Header("Game References")]
    [SerializeField] private FlappyPlayer player;
    [SerializeField] private Spawner spawner;
    [SerializeField] private Text scoreText;
    [SerializeField] private GameObject playButton;
    [SerializeField] private GameObject gameOver;
    [SerializeField] private GameObject gameCompleted;
    [SerializeField] private GameObject loadingSpinner; 
    [SerializeField] private Text timerText; 

    [Header("Scene Settings")]
    [SerializeField] private string npcSceneName = "Test_NPC"; 

    [Header("Audio Settings")]
    public AudioSource musicSource;     // For looping Background Music
    public AudioSource sfxSource;       // For Score Sound effect
    
    public AudioClip backgroundMusic;   
    public AudioClip scoreSound;        
    
    public int score { get; private set; } = 0;
    public static float globalTimer = 0f; 

    private bool isGameActive = false; // Tracks if game is running

    private void Awake() { if (Instance != null) DestroyImmediate(gameObject); else Instance = this; }
    private void OnDestroy() { if (Instance == this) Instance = null; }

    private void Start()
    {
        // 1. Setup Music Source (Looping)
        if (musicSource == null) musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.loop = true;
        musicSource.playOnAwake = true;

        if (backgroundMusic != null)
        {
            musicSource.clip = backgroundMusic;
            musicSource.Play();
        }

        // 2. Setup SFX Source (One Shot)
        if (sfxSource == null) sfxSource = gameObject.AddComponent<AudioSource>();

        Pause();
        if(loadingSpinner) loadingSpinner.SetActive(false);
        if(gameOver) gameOver.SetActive(false);
    }

    private void Update()
    {
        if (Time.timeScale > 0) globalTimer += Time.deltaTime;
        UpdateTimerUI();

        // --- NEW: CHECK FOR PLAYER DESTRUCTION ---
        // If game was active, but player is now null (destroyed), trigger Game Over
        if (isGameActive && player == null)
        {
            GameOver();
        }
        // -----------------------------------------
    }

    private void UpdateTimerUI()
    {
        if (timerText != null)
        {
            int hours = Mathf.FloorToInt(globalTimer / 3600F);
            int minutes = Mathf.FloorToInt((globalTimer % 3600F) / 60F);
            int seconds = Mathf.FloorToInt(globalTimer % 60F);
            timerText.text = (hours > 0) 
                ? string.Format("{0:00}:{1:00}:{2:00}", hours, minutes, seconds)
                : string.Format("{0:00}:{1:00}", minutes, seconds);
        }
    }

    public void Pause()
    {
        Time.timeScale = 0f;
        if(player) player.enabled = false;
        isGameActive = false;
    }

    public void Play()
    {
        // --- NEW: RELOAD IF PLAYER IS GONE ---
        // If the player was destroyed in the last round, we must reload the scene to get them back.
        if (player == null)
        {
            Time.timeScale = 1f; // Unfreeze before reloading
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            return;
        }
        // -------------------------------------

        if(spawner) spawner.ResetSpawner();    
        score = 0;
        if(scoreText) scoreText.text = score.ToString();

        if(playButton) playButton.SetActive(false);
        if(gameOver) gameOver.SetActive(false);
        if (gameCompleted != null) gameCompleted.SetActive(false);
        if (loadingSpinner != null) loadingSpinner.SetActive(false);

        Time.timeScale = 1f;
        if(player) player.enabled = true;
        isGameActive = true;

        Pipes[] pipes = FindObjectsByType<Pipes>(FindObjectsSortMode.None);
        for (int i = 0; i < pipes.Length; i++) { Destroy(pipes[i].gameObject); }
    }

    public void GameOver()
    {
        isGameActive = false;
        if(playButton) playButton.SetActive(true);
        if(!gameOver) gameOver.SetActive(true);
        Pause();
    } 

    public void IncreaseScore()
    {
        score++;
        if(scoreText) scoreText.text = score.ToString();

        if (sfxSource != null && scoreSound != null)
        {
            sfxSource.PlayOneShot(scoreSound);
        }

        if (score >= 25) WinGame();
    }

    private void WinGame()
    {
        isGameActive = false;
        if (gameCompleted != null) gameCompleted.SetActive(true);
        if(playButton) playButton.SetActive(false);
        if (loadingSpinner != null) loadingSpinner.SetActive(true);
        
        if (musicSource) musicSource.Stop();

        Pause(); 

        Debug.Log("🏆 Target Score Reached! Contacting Backend...");

        if (APIManager.Instance != null)
        {
            APIManager.Instance.UnlockNextLevel((success) => 
            {
                if (success)
                {
                    Debug.Log("✅ Level Unlocked!");
                    StartCoroutine(HandleWinSuccess());
                }
                else
                {
                    Debug.LogError("❌ Unlock Failed.");
                    if(playButton) playButton.SetActive(true); 
                    if (loadingSpinner) loadingSpinner.SetActive(false);
                }
            });
        }
        else
        {
            StartCoroutine(HandleWinSuccess());
        }
    }

    IEnumerator HandleWinSuccess()
    {
        GlobalGameState.isReturningFromGame = true;
        
        if (GlobalGameState.activeGameData != null)
        {
            GlobalGameState.completedGames.Add(GlobalGameState.activeGameData.gameName);
        }

        yield return new WaitForSecondsRealtime(2.0f);
        Time.timeScale = 1f; 

        SceneManager.LoadScene(npcSceneName);
    }
    // Add this inside FlappyGameManager class
    public void PlayerCrashed()
    {
        // Prevent double-calling
        if (!isGameActive) return;

        Debug.Log("💥 Player Crashed! Showing Game Over.");
        GameOver();
    }
}
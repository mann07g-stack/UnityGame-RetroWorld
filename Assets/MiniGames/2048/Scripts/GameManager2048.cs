using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement; 
using UnityEngine.UI; 

[DefaultExecutionOrder(-1)]
public class GameManager2048 : MonoBehaviour
{
    public static GameManager2048 Instance { get; private set; }

    [Header("Game References")]
    [SerializeField] private TileBoard board;
    [SerializeField] private CanvasGroup gameOver;
    [SerializeField] private CanvasGroup GameWin;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI hiscoreText;
    
    // --- NEW: Timer Text ---
    [SerializeField] private TextMeshProUGUI timerText; 
    // -----------------------

    [Header("Info Panel UI")]
    [SerializeField] private GameObject infoPanel;      
    [SerializeField] private Button infoButton;         
    [SerializeField] private Button closeInfoButton;    

    [Header("Backend & Scene")]
    [SerializeField] private GameObject loadingSpinner; 
    [SerializeField] private string winSceneName = "Test_NPC"; 

    public int score { get; private set; } = 0;
    private bool hasWon = false; 

    // --- Timer Variables ---
    private float timeElapsed;
    private bool isTimerRunning;

    private void Awake()
    {
        if (Instance != null) { DestroyImmediate(gameObject); } 
        else { Instance = this; }
    }

    private void OnDestroy() { if (Instance == this) Instance = null; }

    private void Start()
    {
        if (infoButton != null) 
        {
            infoButton.onClick.RemoveAllListeners();
            infoButton.onClick.AddListener(OpenInfo);
        }
            
        if (closeInfoButton != null) 
        {
            closeInfoButton.onClick.RemoveAllListeners();
            closeInfoButton.onClick.AddListener(CloseInfo);
        }

        if (infoPanel != null) infoPanel.SetActive(false);

        NewGame();
    }

    private void Update()
    {
        // Only run timer if game is active and not won
        if (isTimerRunning && !hasWon)
        {
            timeElapsed += Time.deltaTime;
            UpdateTimerUI();
        }
    }

    private void UpdateTimerUI()
    {
        if (timerText != null)
        {
            int minutes = Mathf.FloorToInt(timeElapsed / 60F);
            int seconds = Mathf.FloorToInt(timeElapsed % 60F);
            timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        }
    }

    public void OpenInfo()
    {
        if (infoPanel != null)
        {
            isTimerRunning = false; // PAUSE TIMER
            infoPanel.SetActive(true);
            if (board != null) board.enabled = false; 
        }
    }

    public void CloseInfo()
    {
        if (infoPanel != null)
        {
            isTimerRunning = true; // RESUME TIMER
            infoPanel.SetActive(false);
            if (board != null) board.enabled = true; 
        }
    }

    public void NewGame()
    {
        SetScore(0);
        hiscoreText.text = LoadHiscore().ToString();

        // Reset Timer
        timeElapsed = 0f;
        isTimerRunning = true;
        UpdateTimerUI();

        // Fix Invisible Panels Blocking Clicks
        if (gameOver != null)
        {
            gameOver.alpha = 0f;
            gameOver.interactable = false;
            gameOver.blocksRaycasts = false; 
        }

        if (GameWin != null)
        {
            GameWin.alpha = 0f;
            GameWin.interactable = false;
            GameWin.blocksRaycasts = false;
        }

        if(loadingSpinner) loadingSpinner.SetActive(false);
        if(infoPanel) infoPanel.SetActive(false); 
        
        if (infoButton != null) 
        {
            infoButton.interactable = true;
            infoButton.gameObject.SetActive(true);
        }

        hasWon = false;

        if (board != null)
        {
            board.ClearBoard();
            board.CreateTile();
            board.CreateTile();
            board.enabled = true;
        }
    }

    public void GameOver()
    {
        isTimerRunning = false; // STOP TIMER
        
        if (board != null) board.enabled = false;
        
        if (gameOver != null)
        {
            gameOver.interactable = true;
            gameOver.blocksRaycasts = true; 
            StartCoroutine(Fade(gameOver, 1f, 1f));
        }
    }

    public void WinGame()
    {
        if (hasWon) return; 
        hasWon = true;
        isTimerRunning = false; // STOP TIMER

        if (board != null) board.enabled = false;
        
        Debug.Log("🏆 2048 Reached! Contacting Backend...");
        
        if (GameWin != null)
        {
            GameWin.interactable = true;
            GameWin.blocksRaycasts = true; 
            StartCoroutine(Fade(GameWin, 1f, 1f));
        }

        StartCoroutine(WinSequence());
    }

    private IEnumerator WinSequence()
    {
        if (loadingSpinner) loadingSpinner.SetActive(true);

        bool apiSuccess = false;
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
            apiSuccess = true; 
        }

        float timeout = 5f;
        while (timeout > 0 && (loadingSpinner != null && loadingSpinner.activeSelf))
        {
            if (APIManager.Instance == null || apiSuccess) break;
            timeout -= Time.deltaTime;
            yield return null;
        }

        GlobalGameState.isReturningFromGame = true;
        if (GlobalGameState.activeGameData != null)
        {
            GlobalGameState.completedGames.Add(GlobalGameState.activeGameData.gameName);
        }

        yield return new WaitForSeconds(1.5f); 
        SceneManager.LoadScene(winSceneName);
    }

    private IEnumerator Fade(CanvasGroup canvasGroup, float to, float delay = 0f)
    {
        yield return new WaitForSeconds(delay);

        float elapsed = 0f;
        float duration = 0.5f;
        float from = canvasGroup.alpha;

        while (elapsed < duration)
        {
            canvasGroup.alpha = Mathf.Lerp(from, to, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        canvasGroup.alpha = to;
    }

    public void IncreaseScore(int points)
    {
        SetScore(score + points);
    }

    private void SetScore(int score)
    {
        this.score = score;
        if (scoreText != null) scoreText.text = score.ToString();
        SaveHiscore();
    }

    private void SaveHiscore()
    {
        int hiscore = LoadHiscore();
        if (score > hiscore) {
            PlayerPrefs.SetInt("hiscore", score);
        }
    }

    private int LoadHiscore()
    {
        return PlayerPrefs.GetInt("hiscore", 0);
    }
}
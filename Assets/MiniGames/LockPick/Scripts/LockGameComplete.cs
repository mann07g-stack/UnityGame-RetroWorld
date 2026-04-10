using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement; 
using UnityEngine.UI; 

public class LockGameComplete : MonoBehaviour
{
    [Header("Game Parts")]
    public Transform rodPivot;
    public Transform targetPivot;
    public Transform shacklePivot;
    public TextMeshProUGUI counterText; 

    [Header("UI References")]
    public TextMeshProUGUI levelText;   
    public TextMeshProUGUI timerText;   
    
    [Header("Info Panel Settings")]
    public GameObject infoPanel;        

    [Header("Win Screen & Backend")]
    public GameObject winScreenPanel;   
    public TextMeshProUGUI finalTimeText; 
    
    // --- UPDATED: Default Scene is now Test_NPC ---
    public GameObject loadingSpinner;    
    public string winSceneName = "Test_NPC"; // <--- FIXED HERE
    // ---------------------------------------------

    [Header("Game Settings")]
    [SerializeField] private int totalLevels = 10;     
    [SerializeField] private float baseSpeed = 200f;   
    [SerializeField] private float speedIncrease = 80f; 
    [SerializeField] private float hitRange = 20f;
    
    [Header("Audio")]
    public AudioSource sfxSource;       
    public AudioSource musicSource;     
    public AudioClip clickSound;
    public AudioClip errorSound;
    public AudioClip winSound;
    public AudioClip backgroundMusic;   

    // Internal Variables
    private int currentLevel = 1;
    private int clicksLeft;
    private int clicksTotalForThisLevel;
    private float currentRotationSpeed; 
    private float currentDirection;     
    private float targetAngle;
    
    private bool isGameActive = false;
    private bool gameFinished = false;
    private bool isInfoOpen = false;    
    private float totalTimeTimer = 0f;
    
    private Quaternion initialShackleRot;

    void Start()
    {
        if (sfxSource == null) sfxSource = gameObject.AddComponent<AudioSource>();
        
        if (musicSource == null) 
        {
            musicSource = gameObject.AddComponent<AudioSource>();
            musicSource.loop = true;
            musicSource.playOnAwake = true;
        }

        if (backgroundMusic != null)
        {
            musicSource.clip = backgroundMusic;
            musicSource.Play();
        }

        initialShackleRot = shacklePivot.localRotation;
        
        if(winScreenPanel) winScreenPanel.SetActive(false);
        if(infoPanel) infoPanel.SetActive(false); 
        if(loadingSpinner) loadingSpinner.SetActive(false);

        StartNewLevel();
    }

    void Update()
    {
        if (isInfoOpen) return;

        if (!gameFinished)
        {
            totalTimeTimer += Time.deltaTime;
            UpdateTimerUI();
        }

        if (!isGameActive) return;

        rodPivot.Rotate(Vector3.forward * (currentRotationSpeed * currentDirection) * Time.deltaTime);

        if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space))
        {
            CheckHit();
        }
    }

    public void OpenInfoPanel()
    {
        if (gameFinished) return; 
        isInfoOpen = true;
        if(infoPanel) infoPanel.SetActive(true);
    }

    public void CloseInfoPanel()
    {
        isInfoOpen = false;
        if(infoPanel) infoPanel.SetActive(false);
    }

    void StartNewLevel()
    {
        if (currentLevel > totalLevels)
        {
            GameWon();
            return;
        }

        shacklePivot.localRotation = initialShackleRot;
        if(levelText) levelText.text = ""+ currentLevel;

        currentRotationSpeed = baseSpeed + ((currentLevel - 1) * speedIncrease);

        clicksTotalForThisLevel = Random.Range(5, 12);
        clicksLeft = clicksTotalForThisLevel;
        UpdateCounterUI();

        currentDirection = (Random.value > 0.5f) ? 1f : -1f;

        MoveTargetToRandomSpot();
        
        isGameActive = true;
    }

    void MoveTargetToRandomSpot()
    {
        float randomAngle = Random.Range(0f, 360f);
        float currentRodAngle = rodPivot.localEulerAngles.z;
        if (Mathf.Abs(Mathf.DeltaAngle(currentRodAngle, randomAngle)) < 60f)
        {
            randomAngle += 120f; 
        }

        targetAngle = randomAngle;
        targetPivot.localRotation = Quaternion.Euler(0, 0, targetAngle);
    }

    void CheckHit()
    {
        float rodZ = rodPivot.localEulerAngles.z;
        float diff = Mathf.Abs(Mathf.DeltaAngle(rodZ, targetAngle));

        if (diff <= hitRange)
        {
            if(clickSound) sfxSource.PlayOneShot(clickSound);
            
            clicksLeft--;
            UpdateCounterUI();

            if (clicksLeft <= 0)
            {
                StartCoroutine(LevelCompleteSequence());
            }
            else
            {
                MoveTargetToRandomSpot();
                currentDirection *= -1; 
            }
        }
        else
        {
            StartCoroutine(FailAndReset());
        }
    }

    IEnumerator LevelCompleteSequence()
    {
        isGameActive = false;
        if(counterText) counterText.text = ""; 
        
        Quaternion targetRot = initialShackleRot * Quaternion.Euler(0, 0, 35f);
        float t = 0;
        while(t < 0.3f)
        {
            shacklePivot.localRotation = Quaternion.Slerp(initialShackleRot, targetRot, t * 3);
            t += Time.deltaTime;
            yield return null;
        }

        yield return new WaitForSeconds(0.2f);
        
        currentLevel++;
        StartNewLevel();
    }

    IEnumerator FailAndReset()
    {
        isGameActive = false; 
        if(errorSound) sfxSource.PlayOneShot(errorSound);

        Vector3 originalPos = transform.position;
        float t = 0f;
        while(t < 0.3f)
        {
            float x = Random.Range(-0.1f, 0.1f);
            transform.position = originalPos + new Vector3(x, 0, 0);
            t += Time.deltaTime;
            yield return null;
        }
        transform.position = originalPos;

        clicksLeft = clicksTotalForThisLevel;
        UpdateCounterUI();

        yield return new WaitForSeconds(0.5f);
        isGameActive = true; 
    }

    void GameWon()
    {
        gameFinished = true;
        isGameActive = false;
        
        if(winSound) sfxSource.PlayOneShot(winSound);
        if(winScreenPanel) winScreenPanel.SetActive(true);
        if(finalTimeText) finalTimeText.text = "COMPLETED IN: " + FormatTime(totalTimeTimer);

        Debug.Log("🔒 Lockpick Complete! Contacting Backend...");
        
        if(loadingSpinner) loadingSpinner.SetActive(true);

        if (APIManager.Instance != null)
        {
            APIManager.Instance.UnlockNextLevel((success) => 
            {
                if (success)
                {
                    Debug.Log("✅ Backend Confirmed: Level Unlocked!");
                    StartCoroutine(HandleWinSuccess());
                }
                else
                {
                    Debug.LogError("❌ Backend Failed to Unlock.");
                    if(loadingSpinner) loadingSpinner.SetActive(false);
                    if(finalTimeText) finalTimeText.text = "CONNECTION ERROR";
                }
            });
        }
        else
        {
            Debug.LogWarning("⚠️ No APIManager found. Proceeding locally.");
            StartCoroutine(HandleWinSuccess());
        }
    }

    IEnumerator HandleWinSuccess()
    {
        // 1. IMPORTANT: Set the flag so Test_NPC knows we just won
        GlobalGameState.isReturningFromGame = true;
        
        if (GlobalGameState.activeGameData != null)
        {
            GlobalGameState.completedGames.Add(GlobalGameState.activeGameData.gameName);
        }

        yield return new WaitForSecondsRealtime(2.0f);

        Time.timeScale = 1f; 

        // 2. Load Test_NPC (which will auto-play the win dialogue/photo)
        Debug.Log($"🚀 Loading Win Dialogue Scene: {winSceneName}");
        SceneManager.LoadScene(winSceneName);
    }

    void UpdateCounterUI()
    {
        if(counterText) counterText.text = clicksLeft.ToString();
    }

    void UpdateTimerUI()
    {
        if(timerText) timerText.text = FormatTime(totalTimeTimer);
    }

    string FormatTime(float timeInSeconds)
    {
        int minutes = Mathf.FloorToInt(timeInSeconds / 60F);
        int seconds = Mathf.FloorToInt(timeInSeconds % 60F);
        return string.Format("{0:00}:{1:00}", minutes, seconds);
    }
}
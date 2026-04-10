using UnityEngine;
using TMPro;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using System.Linq; 

public class WireManager : MonoBehaviour
{
    [Header("UI Elements")]
    public TMP_Text statusText;
    public TMP_Text levelTimerText;   
    public TMP_Text globalTimerText;  
    public GameObject FlashPanel;     
    public GameObject WinPanel;       
    public GameObject loadingSpinner;
    public List<Button> wireButtons;

    public List<List<WireChildController>> allChildWires = new List<List<WireChildController>>();

    [Header("Wire Colors")]
    public Color[] possibleColors =
    {
        Color.red, Color.blue, Color.yellow, Color.white, Color.green
    };

    [Header("Audio")]
    public AudioSource sfxSource;
    public AudioClip explosionClip;
    public AudioClip winClip;

    [Header("Level Settings")]
    public int maxLevels = 5;
    public string winSceneName = "Test_NPC";

    // Private State
    private float levelTimeRemaining;
    private float globalTimeElapsed; 
    private bool globalTimerRunning = true; 

    private string serialNumber;
    private int batteryCount;
    private int correctWireIndex;
    private bool gameActive = true;
    private int currentLevel = 1;

    void Awake()
    {
        // --- FORCE RESET ON LOAD ---
        Time.timeScale = 1f; 
        
        // Hide panels immediately so they don't block input
        if (FlashPanel != null) FlashPanel.SetActive(false);
        if (WinPanel != null) WinPanel.SetActive(false);
        if (loadingSpinner != null) loadingSpinner.SetActive(false);
    }

    void Start()
    {
        Debug.Log("🔄 Scene Started. Initializing Game...");

        // 1. Find Child Wires
        FindChildWires();

        // 2. Reset Global Stats (Since we reloaded)
        globalTimeElapsed = 0f;
        globalTimerRunning = true;
        currentLevel = 1; 

        // 3. Start First Level
        StartLevel();
    }

    void FindChildWires()
    {
        allChildWires.Clear();
        for (int i = 0; i < wireButtons.Count; i++)
        {
            List<WireChildController> children = new List<WireChildController>();
            if (wireButtons[i] != null)
            {
                foreach (Transform child in wireButtons[i].transform)
                {
                    WireChildController wc = child.GetComponent<WireChildController>();
                    if (wc != null) children.Add(wc);
                }
            }
            allChildWires.Add(children);
        }
    }

    void StartLevel()
    {
        gameActive = true;
        
        // --- MATH TIMER: 50s, 43s, 36s... ---
        float startSeconds = 50f;
        float reductionPerLevel = 7f;
        levelTimeRemaining = startSeconds - ((currentLevel - 1) * reductionPerLevel);
        if (levelTimeRemaining < 10f) levelTimeRemaining = 10f; 

        GenerateScenario();
    }

    void GenerateScenario()
    {
        if (wireButtons == null || wireButtons.Count == 0) return;

        batteryCount = Random.Range(1, 7); 

        string chars = "ABCDEFGHIJKLMNO PQRSTUVWXYZ"; 
        char l1 = chars[Random.Range(0, chars.Length)];
        char l2 = chars[Random.Range(0, chars.Length)];
        int digit = Random.Range(0, 10);
        serialNumber = $"{l1}{l2}{digit}";

        if (statusText)
            statusText.text = $"LVL {currentLevel}/{maxLevels} | SN: {serialNumber} | BATT: {batteryCount}";

        for (int i = 0; i < wireButtons.Count; i++)
        {
            if (wireButtons[i] == null) continue;

            Color randomColor = possibleColors[Random.Range(0, possibleColors.Length)];
            wireButtons[i].image.color = randomColor;
            wireButtons[i].gameObject.name = GetColorName(randomColor);

            if (allChildWires.Count > i)
            {
                foreach (var childWire in allChildWires[i]) childWire.SetColor(randomColor);
            }

            wireButtons[i].interactable = true;
        }

        CalculateAnswer();
    }

    string GetColorName(Color c)
    {
        if (c == Color.red) return "Red";
        if (c == Color.blue) return "Blue";
        if (c == Color.yellow) return "Yellow";
        if (c == Color.white) return "White";
        if (c == Color.green) return "Green";
        return "Unknown";
    }

    void CalculateAnswer()
    {
        string vowels = "AEIOU";
        bool hasVowel = false;
        foreach (char c in serialNumber) { if (vowels.Contains(c.ToString().ToUpper())) { hasVowel = true; break; } }

        List<string> effectiveColors = new List<string>();
        
        for(int i=0; i < wireButtons.Count; i++)
        {
            string realColor = wireButtons[i].gameObject.name;
            string effectiveColor = realColor;

            if (hasVowel)
            {
                if (realColor == "Red") effectiveColor = "Green";
                else if (realColor == "Blue") effectiveColor = "White";
            }
            effectiveColors.Add(effectiveColor);
        }

        Debug.Log($"[Logic] Serial: {serialNumber} (Vowel: {hasVowel}). Effective: {string.Join(",", effectiveColors)}");

        int lastDigit = int.Parse(serialNumber.Substring(serialNumber.Length - 1));
        bool isEven = (lastDigit % 2 == 0);

        // Rule 1: Red Override
        int redCount = effectiveColors.Count(c => c == "Red");
        if (redCount > 1 && isEven) { correctWireIndex = 2; return; }

        // Rule 2: Yellow Absence
        int yellowCount = effectiveColors.Count(c => c == "Yellow");
        if (yellowCount == 0 && batteryCount == 3) { correctWireIndex = 0; return; }

        // Rule 3: White Lead
        if (effectiveColors[0] == "White" && batteryCount < 2) { correctWireIndex = 4; return; }

        // Fail-Safe
        int firstGreenIndex = -1;
        for(int i=0; i<effectiveColors.Count; i++) { if (effectiveColors[i] == "Green") { firstGreenIndex = i; break; } }

        if (firstGreenIndex != -1) correctWireIndex = (firstGreenIndex + batteryCount) % 5;
        else correctWireIndex = 0;
    }

    public void ProcessWireCut(int index)
    {
        if (!gameActive) return;

        if (allChildWires.Count > index)
        {
            foreach (var childWire in allChildWires[index]) childWire.CutWire();
        }

        if (index == correctWireIndex) HandleRoundWin();
        else Explode();
    }

    void Update()
    {
        // Global Timer
        if (globalTimerRunning)
        {
            globalTimeElapsed += Time.deltaTime;
            if (globalTimerText)
            {
                int min = Mathf.FloorToInt(globalTimeElapsed / 60);
                int sec = Mathf.FloorToInt(globalTimeElapsed % 60);
                int ms = Mathf.FloorToInt((globalTimeElapsed * 100) % 100);
                globalTimerText.text = $"{min:00}:{sec:00}.{ms:00}";
            }
        }

        // Level Timer
        if (gameActive)
        {
            levelTimeRemaining -= Time.deltaTime;
            if (levelTimerText) 
                levelTimerText.text = levelTimeRemaining.ToString("F2");

            if (levelTimeRemaining <= 0)
                Explode();
        }
    }

    void HandleRoundWin()
    {
        gameActive = false; 
        DisableAllWires();

        if (currentLevel >= maxLevels) FinalGameWin();
        else StartCoroutine(NextLevelSequence());
    }

    IEnumerator NextLevelSequence()
    {
        if (statusText) statusText.text = "DEFUSED! NEXT BOMB...";
        if (sfxSource && winClip) sfxSource.PlayOneShot(winClip);

        yield return new WaitForSecondsRealtime(2.0f);

        currentLevel++;
        StartLevel();
    }

    void FinalGameWin()
    {
        globalTimerRunning = false; 
        gameActive = false;
        if (sfxSource && winClip) sfxSource.PlayOneShot(winClip);
        if (WinPanel != null) WinPanel.SetActive(true);
        if (statusText) statusText.text = "SYSTEM SECURED";

        StartCoroutine(WinSequence());
    }

    IEnumerator WinSequence()
    {
        yield return new WaitForSecondsRealtime(5f);

        if (loadingSpinner) loadingSpinner.SetActive(true);
        bool apiSuccess = false;
        if (APIManager.Instance != null) APIManager.Instance.UnlockNextLevel((success) => { apiSuccess = success; });
        else apiSuccess = true; 

        float timeout = 5f;
        while (timeout > 0 && (loadingSpinner != null && loadingSpinner.activeSelf))
        {
            if (APIManager.Instance == null || apiSuccess) break;
            timeout -= Time.unscaledDeltaTime;
            yield return null;
        }

        GlobalGameState.isReturningFromGame = true;
        if (GlobalGameState.activeGameData != null) GlobalGameState.completedGames.Add(GlobalGameState.activeGameData.gameName);
            
        SceneManager.LoadScene(winSceneName);
    }

    // ======================
    // DEATH LOGIC
    // ======================
    void Explode()
    {
        if (!gameActive) return;
        
        gameActive = false;
        globalTimerRunning = false; 

        Debug.Log("BOOM! Restarting in 2s...");

        // Start Restart Logic BEFORE audio/visuals to guarantee it runs
        StartCoroutine(RestartAfterDeath());

        // Visuals
        if (sfxSource && explosionClip) sfxSource.PlayOneShot(explosionClip);
        DisableAllWires();
        if (FlashPanel != null) FlashPanel.SetActive(true);
        if (statusText) statusText.text = "YOU DIED";
    }

    IEnumerator RestartAfterDeath()
    {
        // Wait Realtime (ignores any Time.timeScale issues)
        yield return new WaitForSecondsRealtime(2f);
        
        Debug.Log("Restarting Now...");
        
        // Reset Time Scale explicitly before load
        Time.timeScale = 1f;

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    void DisableAllWires()
    {
        if (wireButtons == null) return;
        foreach (Button b in wireButtons)
        {
            if (b != null) b.interactable = false;
        }
    }
}
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.SceneManagement; // Required for Scene Loading

[System.Serializable]
public enum NoteType { Left, Down, Up, Right }

[System.Serializable]
public class NoteData
{
    public float hitTime; // The exact second in the song to be hit
    public NoteType type; // Which arrow/lane
}

public class PianoGameScript : MonoBehaviour
{
    [Header("Prefabs (Left, Down, Up, Right)")]
    public GameObject[] notePrefabs; 

    [Header("Level Mapping")]
    public List<NoteData> playlist = new List<NoteData>();
    
    [Header("Game Settings")]
    public float fallTime = 0.4f; 
    public float targetScore = 5000f;
    public float maxPointsPerNote = 100f;
    public float missPenalty = 50f;
    public bool recordingMode = false;

    [Header("Audio")]
    public AudioSource introSource;
    public AudioSource loopSource;
    
    [Header("UI References")]
    public TextMeshProUGUI scoreText;
    public RectTransform[] laneRefs;   // The target images at bottom
    public RectTransform[] laneSpawns; // The spawn points at top
    public Transform blockParent; 

    // --- NEW: BACKEND UI REFERENCES ---
    [Header("Backend UI")]
    public GameObject winPanel;
    public GameObject loadingSpinner;
    public GameObject losePanel; // Optional
    // ----------------------------------

    [Header("Current State")]
    public float currentScore = 0;
    private float songTime;
    private int noteIndex = 0;
    private bool isLooping = false;
    private bool gameEnded = false;
    private float lastSongTime;
    
    public List<FallingBlock>[] laneBlocks = new List<FallingBlock>[4];

    void Start()
    {
        // 1. Force Time Scale (Fixes pause bugs from previous scenes)
        Time.timeScale = 1f;

        // 2. Hide Backend UI
        if (winPanel != null) winPanel.SetActive(false);
        if (loadingSpinner != null) loadingSpinner.SetActive(false);
        if (losePanel != null) losePanel.SetActive(false);

        // 3. Setup Lanes
        for (int i = 0; i < 4; i++)
            laneBlocks[i] = new List<FallingBlock>();

        // Ensure notes are played in order of time
        playlist.Sort((a, b) => a.hitTime.CompareTo(b.hitTime));

        introSource.Play();
        Invoke(nameof(StartLoop), introSource.clip.length);
        UpdateScoreUI();
    }

    void StartLoop()
    {
        if (gameEnded) return;
        isLooping = true;
        loopSource.loop = true;
        loopSource.Play();
    }

    void UpdateScoreUI()
    {
        if(scoreText) scoreText.text = "Score: " + Mathf.RoundToInt(currentScore) + "/" + targetScore;
    }
    
    void Update(){
        if (gameEnded) return;

        // 1. Calculate Song Time
        if (!isLooping) songTime = introSource.time;
        else songTime = loopSource.time;

        if (songTime < lastSongTime)
        {
            noteIndex = 0;
        }
        lastSongTime = songTime;

        // 2. LOGIC SPLIT: Are we recording or playing?
        if (recordingMode)
        {
            if (Input.GetKeyDown(KeyCode.LeftArrow))  RecordNote(NoteType.Left);
            if (Input.GetKeyDown(KeyCode.DownArrow))  RecordNote(NoteType.Down);
            if (Input.GetKeyDown(KeyCode.UpArrow))    RecordNote(NoteType.Up);
            if (Input.GetKeyDown(KeyCode.RightArrow)) RecordNote(NoteType.Right);
        }
        else
        {
            // Play Mode: Spawning
            if (noteIndex < playlist.Count)
            {
                if (songTime >= playlist[noteIndex].hitTime - fallTime)
                {
                    Spawn((int)playlist[noteIndex].type);
                    noteIndex++;
                }
            }

            // Play Mode: Hitting
            if (Input.GetKeyDown(KeyCode.LeftArrow))  TryHit(0);
            if (Input.GetKeyDown(KeyCode.DownArrow))  TryHit(1);
            if (Input.GetKeyDown(KeyCode.UpArrow))    TryHit(2);
            if (Input.GetKeyDown(KeyCode.RightArrow)) TryHit(3);
        }

        // 3. WIN CONDITION
        if (currentScore >= targetScore) 
        {
            WinGame();
        }
    }

    void TryHit(int lane)
    {
        if (laneBlocks[lane].Count > 0)
        {
            FallingBlock b = laneBlocks[lane][0];
            RectTransform fallingRT = b.GetComponent<RectTransform>();
            RectTransform targetRT = laneRefs[lane];

            float distance = Mathf.Abs(fallingRT.anchoredPosition.y - targetRT.anchoredPosition.y);
            float halfSize = fallingRT.rect.height / 2f;

            if (distance < fallingRT.rect.height)
            {
                float accuracy = Mathf.Max(0, 1 - (distance / halfSize));
                float points = accuracy * maxPointsPerNote;
                
                currentScore += points;
                UpdateScoreUI();
                RemoveBlock(b);
                Destroy(b.gameObject);
            }
        }
    }

    public void HandleMiss(FallingBlock block)
    {
        currentScore = Mathf.Max(0, currentScore - missPenalty);
        UpdateScoreUI();
        RemoveBlock(block);
    }

    public void RemoveBlock(FallingBlock block)
    {
        if (laneBlocks[block.laneIndex].Contains(block))
            laneBlocks[block.laneIndex].Remove(block);
    }

    void Spawn(int lane)
    {
        GameObject go = Instantiate(notePrefabs[lane], laneSpawns[lane]); 
        
        FallingBlock fb = go.GetComponent<FallingBlock>();
        RectTransform rt = go.GetComponent<RectTransform>();

        fb.laneIndex = lane;
        fb.manager = this;

        float dist = Mathf.Abs(laneSpawns[lane].anchoredPosition.y - laneRefs[lane].anchoredPosition.y);
        fb.speed = dist / fallTime;

        rt.anchoredPosition = laneSpawns[lane].anchoredPosition;
        
        laneBlocks[lane].Add(fb);
    }

    void RecordNote(NoteType type){
        NoteData newNote = new NoteData();
        newNote.hitTime = songTime;
        newNote.type = type;
        playlist.Add(newNote);
        Spawn((int)type); 
        Debug.Log("Recorded " + type + " at " + songTime + "s");
    }

    // ==========================================
    //  NEW: BACKEND & WIN LOGIC
    // ==========================================

    void WinGame()
    {
        if (gameEnded) return;
        gameEnded = true;

        Debug.Log("🏆 Target Score Reached!");

        // 1. Stop Audio
        introSource.Stop();
        loopSource.Stop();

        // 2. Show Win UI
        if (winPanel != null) winPanel.SetActive(true);

        // 3. Start Backend Logic
        StartCoroutine(WinSequence());
    }

    IEnumerator WinSequence()
    {
        // 1. Wait 5 Seconds (Visual Flair)
        yield return new WaitForSeconds(5f);

        if (loadingSpinner) loadingSpinner.SetActive(true);

        bool apiSuccess = false;

        // 2. Call API (Unlock Level)
        if (APIManager.Instance != null)
        {
            APIManager.Instance.UnlockNextLevel((success) => 
            {
                apiSuccess = success;
                if(success) Debug.Log("✅ Backend Confirmed!");
            });
        }
        else
        {
            apiSuccess = true; // Fallback
        }

        // 3. Wait for API Response
        float timeout = 5f;
        while (timeout > 0 && (loadingSpinner != null && loadingSpinner.activeSelf))
        {
            if (APIManager.Instance == null || apiSuccess) break;
            timeout -= Time.deltaTime;
            yield return null;
        }

        // 4. Update Global Game State (Mark as Complete)
        GlobalGameState.isReturningFromGame = true;
        if (GlobalGameState.activeGameData != null)
        {
            GlobalGameState.completedGames.Add(GlobalGameState.activeGameData.gameName);
        }
        
        // 5. Return to Correct Map
        string sceneToLoad = GlobalGameState.returnSceneName;
        if (string.IsNullOrEmpty(sceneToLoad)) sceneToLoad = "Test_NPC"; 

        Debug.Log($"🔙 Piano Won. Returning to: {sceneToLoad}");
        SceneManager.LoadScene(sceneToLoad);
    }
}
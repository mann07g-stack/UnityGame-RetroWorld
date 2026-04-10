using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class Lightsout_GridManager : MonoBehaviour
{
    [Header("Game Settings")]
    public int size = 5;
    public GameObject lightPrefab;
    public Transform gridParent; // UI Panel with GridLayoutGroup

    [Header("UI")]
    public GameObject winPanel;
    public GameObject loadingSpinner; // Drag your Loading Spinner here
    public GameObject winText;        // Optional: Keep if you use simple text

    [Header("Audio")]
    public AudioSource musicSource;       // Drag AudioSource for BGM
    public AudioSource sfxSource;         // Drag AudioSource for SFX
    public AudioClip backgroundMusic;     // Drag Loop Music
    public AudioClip winSound;            // Drag Win SFX
    public AudioClip clickSound;          // Drag Click SFX

    [Header("Scene Settings")]
    public string winSceneName = "Test_NPC"; // Scene to return to

    private Lightsout_LightNode[,] grid;
    private bool gameOver = false;

    void Start()
    {
        // Init UI
        if (winPanel != null) winPanel.SetActive(false);
        if (loadingSpinner != null) loadingSpinner.SetActive(false);
        if (winText != null) winText.SetActive(false);

        // Start Music
        if (musicSource != null && backgroundMusic != null)
        {
            musicSource.clip = backgroundMusic;
            musicSource.loop = true;
            musicSource.Play();
        }

        SpawnGrid();
        ScrambleGrid();
    }

    void SpawnGrid()
    {
        // Clean up old grid if any
        foreach (Transform child in gridParent) Destroy(child.gameObject);

        grid = new Lightsout_LightNode[size, size];

        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                GameObject obj = Instantiate(lightPrefab, gridParent);
                Lightsout_LightNode node = obj.GetComponent<Lightsout_LightNode>();
                node.Init(x, y, this);
                grid[x, y] = node;
            }
        }
    }

    public void OnNodeClicked(int x, int y)
    {
        if (gameOver) return;

        // Play Click Sound
        if (sfxSource != null && clickSound != null) sfxSource.PlayOneShot(clickSound);

        Toggle(x, y);       // Center
        Toggle(x + 1, y);   // Right
        Toggle(x - 1, y);   // Left
        Toggle(x, y + 1);   // Up
        Toggle(x, y - 1);   // Down

        CheckWin();
    }

    void Toggle(int x, int y)
    {
        if (x >= 0 && x < size && y >= 0 && y < size)
        {
            grid[x, y].Toggle();
        }
    }

    void CheckWin()
    {
        foreach (Lightsout_LightNode node in grid)
        {
            if (node.isOn) return; // If any light is ON, we haven't won yet
        }

        WinGame();
    }

    void WinGame()
    {
        if (gameOver) return;
        gameOver = true;

        Debug.Log("🏆 Lights Out Solved!");

        // 1. Audio: Stop BGM, Play Win
        if (musicSource != null) musicSource.Stop();
        if (sfxSource != null && winSound != null) sfxSource.PlayOneShot(winSound);

        // 2. UI: Show Win Screen
        if (winPanel != null) winPanel.SetActive(true);
        if (winText != null) winText.SetActive(true);

        // 3. Backend Logic
        StartCoroutine(WinSequence());
    }

    IEnumerator WinSequence()
    {
        // Wait 5 seconds for celebration
        yield return new WaitForSeconds(5.0f);

        if (loadingSpinner != null) loadingSpinner.SetActive(true);

        bool apiSuccess = false;

        // Call API
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

        // Wait for API (Max 5s)
        float timeout = 5f;
        while (timeout > 0 && (loadingSpinner != null && loadingSpinner.activeSelf))
        {
            if (APIManager.Instance == null || apiSuccess) break;
            timeout -= Time.deltaTime;
            yield return null;
        }

        // Update Global State
        GlobalGameState.isReturningFromGame = true;
        if (GlobalGameState.activeGameData != null)
        {
            GlobalGameState.completedGames.Add(GlobalGameState.activeGameData.gameName);
        }

        // Load NPC Scene
        SceneManager.LoadScene(winSceneName);
    }

    void ScrambleGrid()
    {
        // Simple scramble: Simulate valid clicks so the puzzle is always solvable
        int moves = 15;
        for (int i = 0; i < moves; i++)
        {
            int rx = Random.Range(0, size);
            int ry = Random.Range(0, size);
            
            // Toggle logic directly without triggering audio/win checks
            Toggle(rx, ry);
            Toggle(rx + 1, ry);
            Toggle(rx - 1, ry);
            Toggle(rx, ry + 1);
            Toggle(rx, ry - 1);
        }
    }

    public void ResetGame()
    {
        if (gameOver) return; // Optional: Prevent reset if already won? Or allow it.
        
        SpawnGrid();
        ScrambleGrid();
    }
}
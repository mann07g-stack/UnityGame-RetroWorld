using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement;

public class BoardManager : MonoBehaviour
{
    [Header("Board Settings")]
    public int width = 10;
    public int height = 10;
    public int bombCount = 10;
    public float tileSpacing = 1.2f;
    public GameObject tilePrefab;

    [Header("UI")]
    public TextMeshProUGUI bombCounterText;
    public GameObject losePanel;
    public GameObject winPanel;
    public GameObject loadingSpinner; // Drag your Loading Spinner here

    [Header("Timer")]
    public MineTimeManager timerManager; // Ensure this matches your Timer script name

    [Header("Scene Settings")]
    public string winSceneName = "Test_NPC"; // Scene to return to

    [Header("Auto Retry (Loss Only)")]
    public float autoRetryDelay = 5f;

    [Header("Audio")]
    public AudioSource musicSource;       // Drag an AudioSource for Background Music
    public AudioSource sfxSource;         // Drag an AudioSource for SFX
    public AudioClip backgroundMusic;     // Drag Loop Music
    public AudioClip winSound;            // Drag Win SFX
    public AudioClip loseSound;           // Drag Explosion/Lose SFX

    [HideInInspector] public bool gameOver = false;

    private MineTile[,] grid; // Keeps original reference
    private int flagsPlaced = 0;
    private bool hasTriggeredWin = false;

    void Start()
    {
        if (loadingSpinner != null) loadingSpinner.SetActive(false);
        StartNewGame();
    }

    public void StartNewGame()
    {
        StopAllCoroutines();

        gameOver = false;
        hasTriggeredWin = false;
        flagsPlaced = 0;

        // --- AUDIO START ---
        if (musicSource != null && backgroundMusic != null)
        {
            musicSource.clip = backgroundMusic;
            musicSource.loop = true;
            musicSource.Play();
        }
        // -------------------

        if (timerManager != null) timerManager.ResumeTimer();

        if (losePanel != null) losePanel.SetActive(false);
        if (winPanel != null) winPanel.SetActive(false);
        if (loadingSpinner != null) loadingSpinner.SetActive(false);

        UpdateBombCounter();

        ClearBoard();
        GenerateBoard();
        PlaceBombs();
        CalculateNumbers();
    }

    void ClearBoard()
    {
        foreach (Transform child in transform)
            Destroy(child.gameObject);
    }

    void GenerateBoard()
    {
        grid = new MineTile[width, height];

        float offsetX = (width - 1) * tileSpacing / 2f;
        float offsetY = (height - 1) * tileSpacing / 2f;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                Vector2 pos = new Vector2(
                    x * tileSpacing - offsetX,
                    y * tileSpacing - offsetY
                );

                GameObject tileObj = Instantiate(tilePrefab, transform);
                tileObj.transform.localPosition = pos;
                tileObj.name = $"Tile_{x}_{y}";

                MineTile tile = tileObj.GetComponent<MineTile>();
                if (tile != null)
                {
                    tile.Init(this, x, y);
                    grid[x, y] = tile;
                }
            }
        }
    }

    void PlaceBombs()
    {
        int placed = 0;
        int safetyCheck = 0;

        while (placed < bombCount && safetyCheck < 1000)
        {
            safetyCheck++;
            int x = Random.Range(0, width);
            int y = Random.Range(0, height);

            if (!grid[x, y].isBomb)
            {
                grid[x, y].SetBomb();
                placed++;
            }
        }
    }

    void CalculateNumbers()
    {
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (grid[x, y] == null || grid[x, y].isBomb)
                    continue;

                int count = 0;

                for (int ny = -1; ny <= 1; ny++)
                {
                    for (int nx = -1; nx <= 1; nx++)
                    {
                        if (nx == 0 && ny == 0) continue;

                        int cx = x + nx;
                        int cy = y + ny;

                        if (cx >= 0 && cx < width &&
                            cy >= 0 && cy < height &&
                            grid[cx, cy] != null && grid[cx, cy].isBomb)
                        {
                            count++;
                        }
                    }
                }
                grid[x, y].SetAdjacentBombs(count);
            }
        }
    }

    public void FloodFill(int x, int y)
    {
        for (int ny = -1; ny <= 1; ny++)
        {
            for (int nx = -1; nx <= 1; nx++)
            {
                int cx = x + nx;
                int cy = y + ny;

                if (cx < 0 || cx >= width || cy < 0 || cy >= height)
                    continue;

                MineTile tile = grid[cx, cy];

                if (tile == null || tile.isRevealed || tile.isBomb || tile.isFlagged)
                    continue;

                tile.Reveal();

                if (tile.adjacentBombs == 0)
                    FloodFill(cx, cy);
            }
        }
    }

    public void UpdateFlagCount(int delta)
    {
        flagsPlaced += delta;
        UpdateBombCounter();
        CheckWinCondition();
    }

    void UpdateBombCounter()
    {
        if (bombCounterText != null)
            bombCounterText.text = (bombCount - flagsPlaced).ToString();
    }

    void CheckWinCondition()
    {
        if (flagsPlaced != bombCount)
            return;

        foreach (MineTile tile in grid)
        {
            if (tile == null) continue;
            if (tile.isBomb && !tile.isFlagged) return;
            if (!tile.isBomb && tile.isFlagged) return;
        }

        WinGame();
    }

    void WinGame()
    {
        if (gameOver || hasTriggeredWin) return;

        StopAllCoroutines();
        gameOver = true;
        hasTriggeredWin = true;

        // --- AUDIO WIN ---
        if (musicSource != null) musicSource.Stop();
        if (sfxSource != null && winSound != null) sfxSource.PlayOneShot(winSound);
        // -----------------

        if (winPanel != null) winPanel.SetActive(true);
        if (timerManager != null) timerManager.StopTimer();

        Debug.Log("🏆 Minesweeper Solved! Waiting 5s then checking backend...");
        StartCoroutine(WinSequence());
    }

    IEnumerator WinSequence()
    {
        // 1. Wait 5 seconds for celebration
        yield return new WaitForSeconds(5.0f);

        if (loadingSpinner != null) loadingSpinner.SetActive(true);

        bool apiSuccess = false;

        // 2. Call API
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

        // 3. Wait for API (Max 5s)
        float timeout = 5f;
        while (timeout > 0 && (loadingSpinner != null && loadingSpinner.activeSelf))
        {
            if (APIManager.Instance == null || apiSuccess) break;
            timeout -= Time.deltaTime;
            yield return null;
        }

        // 4. Update Global State
        GlobalGameState.isReturningFromGame = true;
        if (GlobalGameState.activeGameData != null)
        {
            GlobalGameState.completedGames.Add(GlobalGameState.activeGameData.gameName);
        }

        SceneManager.LoadScene(winSceneName);
    }

    public void OnBombClicked()
    {
        if (gameOver) return;

        gameOver = true;

        // --- AUDIO LOSE ---
        if (musicSource != null) musicSource.Stop();
        if (sfxSource != null && loseSound != null) sfxSource.PlayOneShot(loseSound);
        // ------------------

        foreach (MineTile tile in grid)
        {
            if(tile != null) tile.RevealBombOnly();
        }

        if (losePanel != null) losePanel.SetActive(true);
        StartCoroutine(AutoRetry());
    }

    IEnumerator AutoRetry()
    {
        yield return new WaitForSeconds(autoRetryDelay);
        StartNewGame();
    }
}
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(GridLayoutGroup))]
public class SudokuGridManager : MonoBehaviour
{
    public GameObject blockPrefab;
    public GameObject cellPrefab;
    public GameObject winPanel;
    public GameObject loadingSpinner; // Drag your Loading Spinner here

    [Header("Audio")]
    public AudioSource musicSource;
    public AudioClip backgroundMusic;

    [Header("Layout")]
    public int gridSize = 9;
    public float boardScreenRatio = 0.85f; // % of smaller screen dimension
    public float spacing = 5f;

    [Header("Scene Settings")]
    public string winSceneName = "Test_NPC"; 

    private SudokuCell selectedCell;
    private SudokuCell[,] cells = new SudokuCell[9, 9];
    private bool gameEnded = false;

    GridLayoutGroup grid;
    RectTransform rt;

    // Hardcoded Puzzle (For testing)
    int[,] puzzle =
    {
        {5,3,0,0,7,0,0,0,0},
        {6,0,0,1,9,5,0,0,0},
        {0,9,8,0,0,0,0,6,0},
        {8,0,0,0,6,0,0,0,3},
        {4,0,0,8,0,3,0,0,1},
        {7,0,0,0,2,0,0,0,6},
        {0,6,0,0,0,0,2,8,0},
        {0,0,0,4,1,9,0,0,5},
        {0,0,0,0,8,0,0,7,9}
    };

    void Awake()
    {
        grid = GetComponent<GridLayoutGroup>();
        rt = GetComponent<RectTransform>();
    }

    void Start()
    {
        if (loadingSpinner != null) loadingSpinner.SetActive(false);
        if (winPanel != null) winPanel.SetActive(false);

        SetupGridSizing();
        PlayMusic();
        GenerateBoard();
    }

    void OnRectTransformDimensionsChange()
    {
        SetupGridSizing();
    }

    void SetupGridSizing()
    {
        float screenMin = Mathf.Min(Screen.width, Screen.height);
        float boardSize = screenMin * boardScreenRatio;

        rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, boardSize);
        rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, boardSize);

        float totalSpacing = spacing * (gridSize - 1);
        float cellSize = (boardSize - totalSpacing) / gridSize;

        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = gridSize;
        grid.spacing = Vector2.one * spacing;
        grid.cellSize = Vector2.one * cellSize;
    }

    void GenerateBoard()
    {
        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                GameObject cellObj = Instantiate(cellPrefab, transform);
                SudokuCell cell = cellObj.GetComponent<SudokuCell>();

                cell.Init(row, col, this);
                cells[row, col] = cell;
            }
        }

        LoadPuzzle();
    }

    void PlayMusic()
    {
        if (musicSource && backgroundMusic)
        {
            musicSource.clip = backgroundMusic;
            musicSource.loop = true;
            musicSource.Play();
        }
    }

    void Update()
    {
        HandleKeyboardInput();
    }

    void HandleKeyboardInput()
    {
        if (gameEnded || selectedCell == null || Keyboard.current == null) return;

        for (int i = 1; i <= 9; i++)
        {
            if (Keyboard.current.digit1Key.wasPressedThisFrame && i == 1) selectedCell.SetValue(1);
            if (Keyboard.current.digit2Key.wasPressedThisFrame && i == 2) selectedCell.SetValue(2);
            if (Keyboard.current.digit3Key.wasPressedThisFrame && i == 3) selectedCell.SetValue(3);
            if (Keyboard.current.digit4Key.wasPressedThisFrame && i == 4) selectedCell.SetValue(4);
            if (Keyboard.current.digit5Key.wasPressedThisFrame && i == 5) selectedCell.SetValue(5);
            if (Keyboard.current.digit6Key.wasPressedThisFrame && i == 6) selectedCell.SetValue(6);
            if (Keyboard.current.digit7Key.wasPressedThisFrame && i == 7) selectedCell.SetValue(7);
            if (Keyboard.current.digit8Key.wasPressedThisFrame && i == 8) selectedCell.SetValue(8);
            if (Keyboard.current.digit9Key.wasPressedThisFrame && i == 9) selectedCell.SetValue(9);
        }

        if (Keyboard.current.backspaceKey.wasPressedThisFrame ||
            Keyboard.current.deleteKey.wasPressedThisFrame ||
            Keyboard.current.digit0Key.wasPressedThisFrame)
        {
            selectedCell.SetValue(0);
        }
    }

    public void SelectCell(SudokuCell cell)
    {
        if (gameEnded) return;

        if (selectedCell != null)
            selectedCell.SetSelected(false);

        selectedCell = cell;
        selectedCell.SetSelected(true);
    }

    void LoadPuzzle()
    {
        for (int r = 0; r < 9; r++)
        {
            for (int c = 0; c < 9; c++)
            {
                if (puzzle[r, c] != 0)
                    cells[r, c].LockCell(puzzle[r, c]);
            }
        }
    }

    public bool IsValidMove(int row, int col, int val)
    {
        if (val == 0) return true;

        for (int i = 0; i < 9; i++)
            if ((i != col && cells[row, i].GetValue() == val) ||
                (i != row && cells[i, col].GetValue() == val))
                return false;

        int br = (row / 3) * 3;
        int bc = (col / 3) * 3;

        for (int r = br; r < br + 3; r++)
            for (int c = bc; c < bc + 3; c++)
                if ((r != row || c != col) && cells[r, c].GetValue() == val)
                    return false;

        return true;
    }

    public bool IsPuzzleSolved()
    {
        for (int r = 0; r < 9; r++)
            for (int c = 0; c < 9; c++)
                if (cells[r, c].GetValue() == 0 || !IsValidMove(r, c, cells[r, c].GetValue()))
                    return false;

        return true;
    }

    public void OnPuzzleSolved()
    {
        if (gameEnded) return;
        gameEnded = true;

        Debug.Log("🏆 Sudoku Solved!");

        if (winPanel != null) winPanel.SetActive(true);
        
        // Stop music if desired (optional)
        // if (musicSource) musicSource.Stop(); 

        StartCoroutine(WinSequence());
    }

    IEnumerator WinSequence()
    {
        // 1. Wait 5 Seconds
        yield return new WaitForSeconds(5f);

        if (loadingSpinner) loadingSpinner.SetActive(true);

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
}
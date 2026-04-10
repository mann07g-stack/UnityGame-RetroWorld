using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class BattleshipGameManager : MonoBehaviour
{
    [Header("Ships")]
    public GameObject[] ships;
    public EnemyScript enemyScript;
    private ShipScript shipScript;
    private List<int[]> enemyShips;
    private int shipIndex = 0;
    public List<TileScript> allTileScripts;    

    [Header("HUD")]
    public Button nextBtn;
    public Button rotateBtn;
    public Button replayBtn;
    public TextMeshProUGUI topText;
    public TextMeshProUGUI playerShipText;
    public TextMeshProUGUI enemyShipText;
    public TextMeshProUGUI timerText;

    [Header("Info Panel UI")]
    public GameObject infoPanel;
    public Button infoBtn;
    public Button closeInfoBtn;

    [Header("Audio Settings")]
    public AudioSource musicSource;      
    public AudioSource sfxSource;        
    public AudioClip backgroundMusic;    
    public AudioClip impactSound;        
    public AudioClip clickSound;         
    public AudioClip winSound; // Optional: Add a win sound
    public AudioClip loseSound; // Optional: Add a lose sound

    [Header("Objects")]
    public GameObject missilePrefab;
    public GameObject enemyMissilePrefab;
    public GameObject firePrefab;
    public GameObject woodDock;

    // --- NEW: Backend & Scene Integration ---
    [Header("Backend & Scene")]
    public GameObject loadingSpinner;    
    public string winSceneName = "Test_NPC"; // Default return scene
    // ----------------------------------------

    private bool setupComplete = false;
    private bool playerTurn = true;
    
    private List<GameObject> playerFires = new List<GameObject>();
    private List<GameObject> enemyFires = new List<GameObject>();
    
    private int enemyShipCount = 5;
    private int playerShipCount = 5;

    // Timer Variables
    private float timeElapsed;
    private bool timerIsRunning = false;
    private bool isGamePaused = false;
    private bool gameEnded = false; // Prevents multiple calls

    void Start()
    {
        shipScript = ships[shipIndex].GetComponent<ShipScript>();
        
        nextBtn.onClick.AddListener(() => { PlaySFX(clickSound); NextShipClicked(); });
        rotateBtn.onClick.AddListener(() => { PlaySFX(clickSound); RotateClicked(); });
        replayBtn.onClick.AddListener(() => { PlaySFX(clickSound); ReplayClicked(); });
        
        infoBtn.onClick.AddListener(() => { PlaySFX(clickSound); OpenInfoClicked(); });
        closeInfoBtn.onClick.AddListener(() => { PlaySFX(clickSound); CloseInfoClicked(); });

        enemyShips = enemyScript.PlaceEnemyShips();
        
        timerIsRunning = true;
        timeElapsed = 0;
        
        infoPanel.SetActive(false);
        if(loadingSpinner) loadingSpinner.SetActive(false);
        replayBtn.gameObject.SetActive(false); // Hide replay initially

        if (musicSource != null && backgroundMusic != null)
        {
            musicSource.clip = backgroundMusic;
            musicSource.loop = true;
            musicSource.volume = 0.5f; 
            musicSource.Play();
        }
    }

    void Update()
    {
        if (timerIsRunning && !isGamePaused && !gameEnded)
        {
            timeElapsed += Time.deltaTime;
            DisplayTime(timeElapsed);
        }
    }

    public void PlaySFX(AudioClip clip)
    {
        if (sfxSource != null && clip != null) sfxSource.PlayOneShot(clip);
    }

    void OpenInfoClicked()
    {
        isGamePaused = true;
        infoPanel.SetActive(true);
        Time.timeScale = 0f;
    }

    void CloseInfoClicked()
    {
        isGamePaused = false;
        infoPanel.SetActive(false);
        Time.timeScale = 1f;
    }

    void DisplayTime(float timeToDisplay)
    {
        timeToDisplay += 1;
        float minutes = Mathf.FloorToInt(timeToDisplay / 60); 
        float seconds = Mathf.FloorToInt(timeToDisplay % 60);
        timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    private void NextShipClicked()
    {
        if (isGamePaused || gameEnded) return;

        if (!shipScript.OnGameBoard())
        {
            shipScript.FlashColor(Color.red);
        } else
        {
            if(shipIndex <= ships.Length - 2)
            {
                shipIndex++;
                shipScript = ships[shipIndex].GetComponent<ShipScript>();
                shipScript.FlashColor(Color.yellow);
            }
            else
            {
                rotateBtn.gameObject.SetActive(false);
                nextBtn.gameObject.SetActive(false);
                woodDock.SetActive(false);
                topText.text = "Guess an enemy tile.";
                setupComplete = true;
                for (int i = 0; i < ships.Length; i++) ships[i].SetActive(false);
            }
        }
    }

    public void TileClicked(GameObject tile)
    {
        if(isGamePaused || gameEnded) return; 

        if(setupComplete && playerTurn)
        {
            Vector3 tilePos = tile.transform.position;
            tilePos.y += 15;
            playerTurn = false;
            Instantiate(missilePrefab, tilePos, missilePrefab.transform.rotation);
        } else if (!setupComplete)
        {
            PlaceShip(tile);
            shipScript.SetClickedTile(tile);
        }
    }

    private void PlaceShip(GameObject tile)
    {
        shipScript = ships[shipIndex].GetComponent<ShipScript>();
        shipScript.ClearTileList();
        Vector3 newVec = shipScript.GetOffsetVec(tile.transform.position);
        ships[shipIndex].transform.localPosition = newVec;
    }

    void RotateClicked()
    {
        if (isGamePaused || gameEnded) return;
        shipScript.RotateShip();
    }

    public void CheckHit(GameObject tile)
    {
        PlaySFX(impactSound);

        int tileNum = Int32.Parse(Regex.Match(tile.name, @"\d+").Value);
        int hitCount = 0;
        foreach(int[] tileNumArray in enemyShips)
        {
            if (tileNumArray.Contains(tileNum))
            {
                for (int i = 0; i < tileNumArray.Length; i++)
                {
                    if (tileNumArray[i] == tileNum)
                    {
                        tileNumArray[i] = -5;
                        hitCount++;
                    }
                    else if (tileNumArray[i] == -5)
                    {
                        hitCount++;
                    }
                }
                if (hitCount == tileNumArray.Length)
                {
                    enemyShipCount--;
                    topText.text = "SUNK!!!!!!";
                    enemyFires.Add(Instantiate(firePrefab, tile.transform.position, Quaternion.identity));
                    tile.GetComponent<TileScript>().SetTileColor(1, new Color32(68, 0, 0, 255));
                    tile.GetComponent<TileScript>().SwitchColors(1);
                }
                else
                {
                    topText.text = "HIT!!";
                    tile.GetComponent<TileScript>().SetTileColor(1, new Color32(255, 0, 0, 255));
                    tile.GetComponent<TileScript>().SwitchColors(1);
                }
                break;
            }
            
        }
        if(hitCount == 0)
        {
            tile.GetComponent<TileScript>().SetTileColor(1, new Color32(38, 57, 76, 255));
            tile.GetComponent<TileScript>().SwitchColors(1);
            topText.text = "Missed.";
        }
        
        // --- CHECK WIN CONDITION IMMEDIATELY ---
        if (enemyShipCount < 1) 
        {
            GameOver("YOU WIN!!");
        }
        else
        {
            Invoke("EndPlayerTurn", 1.0f);
        }
    }

    public void EnemyHitPlayer(Vector3 tile, int tileNum, GameObject hitObj)
    {
        PlaySFX(impactSound);

        enemyScript.MissileHit(tileNum);
        tile.y += 0.2f;
        playerFires.Add(Instantiate(firePrefab, tile, Quaternion.identity));
        if (hitObj.GetComponent<ShipScript>().HitCheckSank())
        {
            playerShipCount--;
            playerShipText.text = playerShipCount.ToString();
            enemyScript.SunkPlayer();
        }
        
        // --- CHECK LOSE CONDITION IMMEDIATELY ---
        if (playerShipCount < 1)
        {
            GameOver("ENEMY WINs!!!");
        }
        else
        {
            Invoke("EndEnemyTurn", 2.0f);
        }
    }

    private void EndPlayerTurn()
    {
        if(gameEnded) return; // Safety check
        
        for (int i = 0; i < ships.Length; i++) ships[i].SetActive(true);
        foreach (GameObject fire in playerFires) fire.SetActive(true);
        foreach (GameObject fire in enemyFires) fire.SetActive(false);
        enemyShipText.text = enemyShipCount.ToString();
        topText.text = "Enemy's turn";
        enemyScript.NPCTurn();
        ColorAllTiles(0);
    }

    public void EndEnemyTurn()
    {
        if(gameEnded) return; // Safety check

        for (int i = 0; i < ships.Length; i++) ships[i].SetActive(false);
        foreach (GameObject fire in playerFires) fire.SetActive(false);
        foreach (GameObject fire in enemyFires) fire.SetActive(true);
        playerShipText.text = playerShipCount.ToString();
        topText.text = "Select a tile";
        playerTurn = true;
        ColorAllTiles(1);
    }

    private void ColorAllTiles(int colorIndex)
    {
        foreach (TileScript tileScript in allTileScripts)
        {
            tileScript.SwitchColors(colorIndex);
        }
    }

    // =========================================================
    //  UPDATED GAME OVER LOGIC
    // =========================================================
    void GameOver(string winner)
    {
        gameEnded = true;
        timerIsRunning = false; 
        playerTurn = false;
        CancelInvoke(); // Stop any pending turn switches

        if (winner == "YOU WIN!!")
        {
            topText.text = "VICTORY!";
            PlaySFX(winSound);
            StartCoroutine(WinGameSequence());
        }
        else
        {
            topText.text = "DEFEAT!";
            PlaySFX(loseSound);
            // Show Replay Button only on Defeat
            replayBtn.gameObject.SetActive(true);
        }
    }

    IEnumerator WinGameSequence()
    {
        Debug.Log("🚢 Battleship Won! Contacting Backend...");
        
        if (loadingSpinner) loadingSpinner.SetActive(true);
        
        // Disable UI interactions
        replayBtn.gameObject.SetActive(false); 

        // 1. Call API
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
            apiSuccess = true; // Local testing fallback
        }

        // 2. Wait for API (max 5 seconds timeout)
        float timeout = 5f;
        while (timeout > 0 && loadingSpinner.activeSelf)
        {
            if (APIManager.Instance == null || apiSuccess) break; // Break if done
            timeout -= Time.deltaTime;
            yield return null;
        }

        // 3. Mark Global State
        GlobalGameState.isReturningFromGame = true;
        if (GlobalGameState.activeGameData != null)
        {
            GlobalGameState.completedGames.Add(GlobalGameState.activeGameData.gameName);
        }

        // 4. Return to NPC Scene (to show Photo/Dialogue)
        Debug.Log($"🚀 Loading Return Scene: {winSceneName}");
        yield return new WaitForSeconds(1.0f); // Small delay for effect
        SceneManager.LoadScene(winSceneName);
    }

    void ReplayClicked()
    {
        // Simple scene reload for retry
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
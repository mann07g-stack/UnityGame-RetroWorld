using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class CrossyGameManager : MonoBehaviour
{
    public static CrossyGameManager Instance;

    [Header("Game References")]
    public CrossyPlayerController player; 
    public Transform startPoint;

    [Header("UI & Backend")]
    public GameObject winScreenPanel;   
    public GameObject loadingSpinner;   
    public string winSceneName = "Test_NPC"; 

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        if (winScreenPanel) winScreenPanel.SetActive(false);
        if (loadingSpinner) loadingSpinner.SetActive(false);
    }

    public void ResetToStart()
    {
        player.transform.position = startPoint.position;
        player.isAlive = true;
    }

    public void WinGame()
    {
        if (player) player.isAlive = false;
        if (winScreenPanel) winScreenPanel.SetActive(true);

        Debug.Log("🏆 Crossy Road Won! Contacting Backend...");
        StartCoroutine(WinSequence());
    }

    IEnumerator WinSequence()
    {
        if (loadingSpinner) loadingSpinner.SetActive(true);

        bool apiSuccess = false;
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
            apiSuccess = true; 
        }

        float timeout = 5f;
        while (timeout > 0 && loadingSpinner.activeSelf)
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

        yield return new WaitForSeconds(1.0f); 

        Time.timeScale = 1f; 
        SceneManager.LoadScene(winSceneName);
    }
}
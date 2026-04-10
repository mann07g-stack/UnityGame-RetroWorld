using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class PlayerDataManager : MonoBehaviour
{
    public static PlayerDataManager Instance;

    public string currentPlayerName = "";
    public string currentAppNumber = "";// Stores the current player's name
    public List<PlayerResult> leaderboard = new List<PlayerResult>(); // Server-fetched results

    private bool hasSavedCurrentPlayer = false; // Prevents duplicate upload

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(this.gameObject); // Persist across scenes
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Called after final level to upload the score
    public void SaveCurrentPlayerResult()
    {
        if (!hasSavedCurrentPlayer && SceneManager.GetActiveScene().name == "LEVEL02")
        {
            int finalScore = GameStatsManager.Instance.totalCollisions;
            float timeTaken = GameTimer.Instance.GetElapsedTime();

            hasSavedCurrentPlayer = true;

            LeaderboardUploader uploader = FindFirstObjectByType<LeaderboardUploader>();
            if (uploader != null)
            {
                StartCoroutine(uploader.UploadScore(currentAppNumber, finalScore, timeTaken));
            }
            else
            {
                Debug.LogWarning("🚨 LeaderboardUploader not found in scene!");
            }
        }
    }

    // Update leaderboard when new data is fetched
    public void UpdateLeaderboard(List<PlayerResult> serverData)
    {
        leaderboard = serverData;
    }

    // Called from LEVEL01 after loading player name from menu scene
    public void SetCurrentPlayer(string name, string appNumber)
    {
        currentPlayerName = name;
        currentAppNumber = appNumber;
        hasSavedCurrentPlayer = false; // Reset for new game session
        GameStatsManager.Instance.totalCollisions = 0;
    }

    // Check before uploading score
    public bool IsCurrentPlayerValid()
    {
        return !string.IsNullOrEmpty(currentPlayerName);
    }
    // Prevents duplicate names (case-insensitive)
    public bool IsNameTaken(string name)
    {
        return leaderboard.Exists(entry =>
            entry != null &&
            entry.name != null &&
            entry.name.Equals(name, System.StringComparison.OrdinalIgnoreCase));
    }
    public bool IsAppNumberTaken(string appNo)
    {
        return leaderboard.Exists(entry =>
            entry != null &&
            entry.appNo != null &&
            entry.appNo.Equals(appNo, System.StringComparison.OrdinalIgnoreCase));
    }
    public string GetNameFromAppNumber(string appNo)
    {
        foreach (PlayerResult entry in leaderboard)
        {
            if (entry != null && entry.appNo != null &&
                entry.appNo.Equals(appNo, System.StringComparison.OrdinalIgnoreCase))
            {
                return entry.name;
            }
        }

        Debug.LogWarning("App number not found in leaderboard.");
        return "";
    }
    public void ClearCurrentPlayer()
    {
        currentPlayerName = "";
        currentAppNumber = "";
    }
    
   


    

}

using UnityEngine;
using TMPro;
using System.Collections;
using System.Linq;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;
using System.Collections.Generic;

public class PostGameSummaryUI : MonoBehaviour
{
    public TMP_Text nameText;
    public TMP_Text scoreText;
    public TMP_Text rankText;
    public TMP_Text levelsText;

    public string serverURL = "https://leaderboard-avwu.onrender.com";

    void Start()
    {
        string playerName = PlayerDataManager.Instance.currentPlayerName;
        int totalCollisions = GameStatsManager.Instance.totalCollisions;
        float completionTime = GameStatsManager.Instance.levelTime;
        int levelsCompleted = GameStatsManager.Instance.levelsCompleted;

        // ✅ Display on UI
        nameText.text = $"Name: {playerName}";
        scoreText.text = $"Collisions: {totalCollisions}";
        levelsText.text = $"Levels Completed: {levelsCompleted}";

        // ✅ Get App Number
        string appNumber = PlayerPrefs.GetString("AppNumber", "");

        // 🔍 Debugging Logs
        Debug.Log("📦 AppNumber from PlayerPrefs: " + appNumber);
        Debug.Log($"📊 Game Stats - Collisions: {totalCollisions}, Time: {completionTime}, Levels Completed: {levelsCompleted}");

        if (string.IsNullOrEmpty(appNumber))
        {
            Debug.LogError("❌ App number not found.");
            rankText.text = "Rank: N/A";
            return;
        }

        if (totalCollisions >= 0 && completionTime >= 0f)
        {
            StartCoroutine(SubmitScoreAndFetchRank(appNumber, playerName, totalCollisions, completionTime, levelsCompleted));
        }
    }

    IEnumerator SubmitScoreAndFetchRank(string appNumber, string playerName, int collisions, float time, int levelsCompleted)
    {
        appNumber = appNumber.Trim();

        ScoreSubmission submission = new ScoreSubmission(appNumber, collisions, time, levelsCompleted);
        string json = JsonUtility.ToJson(submission);

        Debug.Log($"🚀 Submitting JSON: {json}");

        UnityWebRequest submitRequest = new UnityWebRequest(serverURL + "/submit", "POST");
        byte[] body = System.Text.Encoding.UTF8.GetBytes(json);
        submitRequest.uploadHandler = new UploadHandlerRaw(body);
        submitRequest.downloadHandler = new DownloadHandlerBuffer();
        submitRequest.SetRequestHeader("Content-Type", "application/json");

        yield return submitRequest.SendWebRequest();

        if (submitRequest.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("❌ Score submission failed: " + submitRequest.downloadHandler.text);
            rankText.text = "Rank: N/A";
            yield break;
        }

        // ✅ Get updated leaderboard
        UnityWebRequest getRequest = UnityWebRequest.Get(serverURL + "/leaderboard-data");
        yield return getRequest.SendWebRequest();

        if (getRequest.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("❌ Failed to get leaderboard: " + getRequest.error);
            rankText.text = "Rank: N/A";
            yield break;
        }

        string leaderboardJson = getRequest.downloadHandler.text;
        PlayerData[] players = JsonHelper.FromJson<PlayerData>(leaderboardJson);

        var sorted = players
            .Where(p => !string.IsNullOrEmpty(p.name))
            .OrderByDescending(p => p.levels_completed)
            .ThenBy(p => p.collisions)
            .ThenBy(p => p.time)
            .ToList();

        int rank = sorted.FindIndex(p =>
            p.name == playerName &&
            p.collisions == collisions &&
            Mathf.Approximately(p.time, time)
        ) + 1;

        rankText.text = rank > 0 ? $"Rank: #{rank}" : "Rank: N/A";
    }

    public void OnNextClicked()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }


    [System.Serializable]
    public class PlayerData
    {
        public string name;
        public string application_number;
        public int collisions;
        public float time;
        public int levels_completed;
    }

    public static class JsonHelper
    {
        public static T[] FromJson<T>(string json)
        {
            string newJson = "{\"array\":" + json + "}";
            return JsonUtility.FromJson<Wrapper<T>>(newJson).array;
        }

        [System.Serializable]
        private class Wrapper<T>
        {
            public T[] array;
        }
    }

    [System.Serializable]
    public class ScoreSubmission
    {
        public string application_number;
        public int collisions;
        public float time;
        public int levels_completed;

        public ScoreSubmission(string appNumber, int collisions, float time, int levelsCompleted)
        {
            this.application_number = appNumber;
            this.collisions = collisions;
            this.time = time;
            this.levels_completed = levelsCompleted;
        }
    }
}

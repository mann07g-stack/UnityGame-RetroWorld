using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class LeaderboardUploader : MonoBehaviour
{
    private string submitUrl = "https://leaderboard-avwu.onrender.com/submit";
    private string getUrl = "https://leaderboard-avwu.onrender.com/leaderboard";

    public IEnumerator UploadScore(string appNo, int collisions, float time)
    {
        ScoreData data = new ScoreData();

        data.application_number = appNo;
        data.collisions = collisions;
        data.time = time;
        

        string jsonData = JsonUtility.ToJson(data);

        UnityWebRequest request = new UnityWebRequest(submitUrl, "POST");
        byte[] bodyRaw = new System.Text.UTF8Encoding().GetBytes(jsonData);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Upload failed: " + request.error);
        }
        else
        {
            Debug.Log("✅ Upload successful!");
        }
    }

    public IEnumerator FetchLeaderboard(System.Action<string> onSuccess)
    {
        UnityWebRequest request = UnityWebRequest.Get(getUrl);
        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Failed to fetch leaderboard: " + request.error);
        }
        else
        {
            Debug.Log("✅ Leaderboard data received.");
            onSuccess?.Invoke(request.downloadHandler.text);
        }
    }

    [System.Serializable]
    public class ScoreData
    {
        public string application_number;
        public int collisions;
        public float time;
    }
}

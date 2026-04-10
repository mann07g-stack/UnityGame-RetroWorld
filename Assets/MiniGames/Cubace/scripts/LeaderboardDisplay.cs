using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;

public class LeaderboardDisplay : MonoBehaviour
{
    public string serverURL = "https://leaderboard-avwu.onrender.com"; // ✅ Replace with your Render URL
    public TextMeshProUGUI leaderboardText;

    void Start()
    {
        StartCoroutine(GetLeaderboard());
    }

    IEnumerator GetLeaderboard()
    {
        UnityWebRequest request = UnityWebRequest.Get(serverURL + "/leaderboard");

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Failed to get leaderboard: " + request.error);
        }
        else
        {
            string json = request.downloadHandler.text;
            PlayerData[] players = JsonHelper.FromJson<PlayerData>(json);
            string display = "Name\tApp No\tCollisions\tTime\n";

            foreach (var p in players)
            {
                display += $"{p.name}\t{p.appNo}\t{p.collisions}\t{p.time}\n";
            }

            leaderboardText.text = display;
        }
    }

    [System.Serializable]
    public class PlayerData
    {
        public string name;
        public string appNo;
        public int collisions;
        public float time;
    }

    public static class JsonHelper
    {
        public static T[] FromJson<T>(string json)
        {
            string newJson = "{\"array\":" + json + "}";
            Wrapper<T> wrapper = JsonUtility.FromJson<Wrapper<T>>(newJson);
            return wrapper.array;
        }

        [System.Serializable]
        private class Wrapper<T>
        {
            public T[] array;
        }
    }
}

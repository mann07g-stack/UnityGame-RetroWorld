using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class RetryLogin : MonoBehaviour
{
    [System.Serializable]
    public class PlayerData
    {
        public string name;
        public string application_number;
        public int collisions;
        public float time;
        public int levelsCompleted;
    }



    public TMP_InputField retryAppInputField;
    public TextMeshProUGUI invalidAppWarning;
    public string serverURL = "https://leaderboard-avwu.onrender.com"; // ✅ Replace with your actual server

    void Start()
    {
        invalidAppWarning.gameObject.SetActive(false);
    }

    public void OnRetryButtonClick()
    {
        string appNo = retryAppInputField.text.Trim();
        if (string.IsNullOrEmpty(appNo))
        {
            Debug.LogWarning("Enter Application Number.");
            return;
        }

        StartCoroutine(FetchAndValidate(appNo));
    }

    IEnumerator FetchAndValidate(string appNo)
    {
        UnityWebRequest request = UnityWebRequest.Get(serverURL + "/registered-players");

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Failed to fetch leaderboard: " + request.error);
            invalidAppWarning.text = "Server Error!";
            invalidAppWarning.gameObject.SetActive(true);
            yield break;
        }

        string json = request.downloadHandler.text;
        Debug.Log("RAW JSON: " + json);

        List<PlayerData> leaderboard = JsonHelper.FromJson<PlayerData>(json).ToList();


        PlayerData matchedPlayer = leaderboard.FirstOrDefault(p => p.application_number.Trim() == appNo);

        if (matchedPlayer == null)
        {
            Debug.LogWarning("Invalid Application Number.");
            invalidAppWarning.text = "Application Number Not Registered!";
            invalidAppWarning.gameObject.SetActive(true);
            yield break;
        }

        // ✅ Valid retry
        Debug.Log("Logged in as: " + matchedPlayer.name + " (" + matchedPlayer.application_number + ")");
        PlayerDataManager.Instance.SetCurrentPlayer(matchedPlayer.name, matchedPlayer.application_number);
        GameStatsManager.Instance.totalCollisions = 0;
        SceneManager.LoadScene("Loading Screen"); // Or "LEVEL01"
    }
}

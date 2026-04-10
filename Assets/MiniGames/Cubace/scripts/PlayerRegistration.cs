using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement;

public class PlayerRegistration : MonoBehaviour
{
    public TMP_InputField nameInput;
    public TMP_InputField appNumberInput;
    public TMP_Text statusText;
    public string serverUrl = "https://leaderboard-avwu.onrender.com";

    public void RegisterPlayer()
    {
        string name = nameInput.text.Trim();
        string appNumber = appNumberInput.text.Trim();

        // 🧼 Clear previous status
        if (statusText) statusText.text = "";

        if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(appNumber))
        {
            Debug.LogError("❌ Name and Application Number are required.");
            if (statusText) statusText.text = "Please enter both name and application number.";
            return;
        }

        StartCoroutine(SendRegistration(name, appNumber));
    }

    IEnumerator SendRegistration(string name, string appNumber)
    {
        PlayerData data = new PlayerData
        {
            name = name,
            application_number = appNumber
        };

        string json = JsonUtility.ToJson(data);
        Debug.Log("📨 Sending registration JSON: " + json);

        UnityWebRequest request = new UnityWebRequest(serverUrl + "/register", "POST");
        byte[] body = System.Text.Encoding.UTF8.GetBytes(json);
        request.uploadHandler = new UploadHandlerRaw(body);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            string errorResponse = request.downloadHandler.text;
            Debug.LogError("❌ Registration failed: " + errorResponse);

            if (statusText)
            {
                if (errorResponse.Contains("Name already taken"))
                {
                    statusText.text = "Name already taken.";
                }
                else if (errorResponse.Contains("Application number already used"))
                {
                    statusText.text = "Application number already registered.";
                }
                else
                {
                    statusText.text = "Registration failed. Try again.";
                }
            }
        }
        else
        {
            Debug.Log("✅ Registration successful!");

            // Save data only after confirmation from server
            PlayerPrefs.SetString("AppNumber", appNumber);

            if (PlayerDataManager.Instance != null)
            {
                PlayerDataManager.Instance.SetCurrentPlayer(name, appNumber);
            }

            if (statusText) statusText.text = "Registered! Starting game...";
            yield return new WaitForSeconds(1f);

            SceneManager.LoadScene("Loading Screen"); // Or "LEVEL01"
        }
    }

    [System.Serializable]
    public class PlayerData
    {
        public string name;
        public string application_number;
    }
}

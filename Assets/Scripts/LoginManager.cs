using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;
using System.Collections;

public class LoginManager : MonoBehaviour
{
    [Header("UI References")]
    public TMP_InputField teamIdInput;
    public TMP_InputField passwordInput;
    public Button loginButton;
    public TextMeshProUGUI errorText;
    public GameObject loadingSpinner;

    // FIX 1: Point to Root (No /auth)
    private string baseUrl = "http://localhost:3000/api"; 

    public void OnLoginClick()
    {
        string id = teamIdInput.text.Trim();
        string pass = passwordInput.text.Trim();

        if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(pass))
        {
            errorText.text = "Please enter both ID and Password.";
            return;
        }

        StartCoroutine(LoginRoutine(id, pass));
    }

    IEnumerator LoginRoutine(string teamId, string password)
    {
        loginButton.interactable = false;
        if(loadingSpinner) loadingSpinner.SetActive(true);
        errorText.text = "Authenticating...";

        // FIX 2: Generate Device ID
        string deviceId = SystemInfo.deviceUniqueIdentifier;

        // FIX 3: Add deviceId to JSON
        string jsonBody = $"{{\"teamId\":\"{teamId}\", \"password\":\"{password}\", \"deviceId\":\"{deviceId}\"}}";

        // FIX 4: Correct URL (Base + /login)
        string url = $"{baseUrl}/login";

        Debug.Log($"[LOGIN] Sending to {url} with DeviceID: {deviceId}");

        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonBody);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                string jsonResponse = request.downloadHandler.text;
                Debug.Log("Login Success: " + jsonResponse);

                LoginResponse data = JsonUtility.FromJson<LoginResponse>(jsonResponse);

                if (!string.IsNullOrEmpty(data.token))
                {
                    GlobalGameState.playerToken = data.token;
                    GlobalGameState.teamId = data.teamId; 
                    string currentDeviceId = SystemInfo.deviceUniqueIdentifier;
                    GlobalGameState.deviceId = currentDeviceId;
                    SceneManager.LoadScene("MainMap");
                }
                else
                {
                    errorText.text = "Error: Token missing from server response.";
                }
            }
            else
            {
                // FIX 5: Show the REAL error from the server (e.g. "Device ID required")
                string serverMsg = request.downloadHandler.text;
                Debug.LogError($"Login Failed: {serverMsg}");

                if (request.responseCode == 403)
                {
                    errorText.text = "Max 4 Devices Reached.\nLog out elsewhere.";
                }
                else if (serverMsg.Contains("Device ID")) 
                {
                    errorText.text = "Update App: Missing Device ID";
                }
                else if (request.responseCode == 401 || request.responseCode == 400)
                {
                    errorText.text = "Invalid ID or Password.";
                }
                else
                {
                    errorText.text = "Server Connection Error.";
                }
            }
        }

        loginButton.interactable = true;
        if(loadingSpinner) loadingSpinner.SetActive(false);
    }

    [System.Serializable]
    public class LoginResponse
    {
        public string token;
        public string teamId;
        public string name;
    }
}
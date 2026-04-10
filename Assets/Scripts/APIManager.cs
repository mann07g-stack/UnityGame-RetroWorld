using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class APIManager : MonoBehaviour
{
    public static APIManager Instance { get; private set; }
    private string baseUrl = "http://localhost:3000/api";
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); // Destroy duplicates if we return to main scene
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // <--- CRITICAL: Keeps this object alive between scenes
        }
    }

    // --- 1. GET QUESTION ---
    public void GetQuestion(int gameId, Action<QuestionResponse> onSuccess, Action<string> onFailure)
    {
        StartCoroutine(GetQuestionRoutine(gameId, onSuccess, onFailure));
    }

    IEnumerator GetQuestionRoutine(int gameId, Action<QuestionResponse> onSuccess, Action<string> onFailure)
    {
        string url = $"{baseUrl}/get-question/";

        // --------------------------------------------

        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            // CRITICAL: Attach the Token
            request.SetRequestHeader("Authorization", "Bearer " + GlobalGameState.playerToken);
            request.SetRequestHeader("Content-Type", "application/json");

            Debug.Log($"[API] Requesting Question: {url}");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                string json = request.downloadHandler.text;
                Debug.Log($"[API] Question Received: {json}");
                
                try 
                {
                    QuestionResponse response = JsonUtility.FromJson<QuestionResponse>(json);
                    onSuccess?.Invoke(response);
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"JSON Parse Error: {e.Message}");
                    onFailure?.Invoke("Data Error");
                }
            }
            else
            {
                // This prints exactly WHY it failed (401 vs 404 vs 500)
                Debug.LogError($"[API ERROR] {request.responseCode} : {request.error}");
                onFailure?.Invoke(request.error); 
            }
        }
    }
    // --- 2. SUBMIT ANSWER ---
    // --- 2. SUBMIT ANSWER (Fixing the Missing Token) ---
    public void SubmitAnswer(int questionId, string hashedAnswer, System.Action<bool> onResult)
    {
        StartCoroutine(SubmitAnswerRoutine(questionId, hashedAnswer, onResult));
    }

    IEnumerator SubmitAnswerRoutine(int questionId, string answer, System.Action<bool> onResult)
    {
        string url = $"{baseUrl}/submit-answer";
        
        // --- UPDATED JSON: INCLUDE LOCATION ID ---
        string currentLocation = GlobalGameState.currentScannedLocation;
        Debug.Log($"📤 SENDING TO BACKEND: LocationID = '{currentLocation}' (Should be your First Location ID)");
        string jsonBody = $"{{\"questionId\": {questionId}, \"hashedAnswer\": \"{answer}\", \"scannedLocationId\": \"{currentLocation}\"}}";

        using (UnityEngine.Networking.UnityWebRequest request = new UnityEngine.Networking.UnityWebRequest(url, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonBody);
            request.uploadHandler = new UnityEngine.Networking.UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new UnityEngine.Networking.DownloadHandlerBuffer();
            
            if (string.IsNullOrEmpty(GlobalGameState.playerToken))
            {
                onResult?.Invoke(false);
                yield break;
            }

            request.SetRequestHeader("Authorization", "Bearer " + GlobalGameState.playerToken);
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result == UnityEngine.Networking.UnityWebRequest.Result.Success)
            {
                string json = request.downloadHandler.text;
                // If location was wrong, backend sends { "correct": false }
                if (json.Contains("\"correct\":true"))
                {
                    onResult?.Invoke(true);
                }
                else
                {
                    // This now handles WRONG ANSWER + WRONG LOCATION
                    onResult?.Invoke(false);
                }
            }
            else
            {
                onResult?.Invoke(false);
            }
        }
    }
    // --- 3. UNLOCK NEXT LEVEL ---
    // 1. Make sure the method name matches exactly what MiniGameSession calls
    public void UnlockNextLevel(System.Action<bool> onResult)
    {
        StartCoroutine(UnlockRoutine(onResult));
    }

    IEnumerator UnlockRoutine(System.Action<bool> onResult)
    {
        // 2. CHECK URL: Ensure this matches your Spring Boot Controller
        // Example: @PostMapping("/unlock-next")
        string url = $"{baseUrl}/unlock-next"; 

        Debug.Log($"[API] POST Request to: {url}");

        // 3. Create Empty JSON (or add body if your server needs it)
        // Many 'Unlock' endpoints are just POSTs with no body, relying on the Token to ID the user.
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes("{}"); 

        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            
            // 4. ATTACH TOKEN
            request.SetRequestHeader("Authorization", "Bearer " + GlobalGameState.playerToken);
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log($"[API] Response: {request.downloadHandler.text}");
                onResult?.Invoke(true);
            }
            else
            {
                Debug.LogError($"[API ERROR] {request.responseCode}: {request.error}");
                Debug.LogError($"[API RESPONSE] {request.downloadHandler.text}");
                
                // If 401: Token is wrong.
                // If 404: URL is wrong.
                onResult?.Invoke(false);
            }
        }
    }

    IEnumerator UnlockNextLevelRoutine(Action<bool> onResult)
    {
        string url = $"{baseUrl}/unlock-next";

        // Even if the body is empty, we often send an empty JSON object just in case
        // or just send a POST with no body depending on your backend. 
        // We will send an empty POST here.
        string token = GlobalGameState.playerToken;
        Debug.Log($"[API DEBUG] Token being sent: 'Bearer {token}'");
        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            // We still need the Token to know WHO is unlocking the level
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Authorization", "Bearer " + GlobalGameState.playerToken);
            request.SetRequestHeader("Content-Type", "application/json"); 

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("[API] Next Level Unlocked!");
                onResult?.Invoke(true);
            }
            else
            {
                Debug.LogError($"[API FAIL] Unlock Failed: {request.error}");
                onResult?.Invoke(false);
            }
        }
    }
    // ... inside APIManager class ...

    // --- 4. VERIFY LOCATION ---
    // Inside APIManager.cs

    // Change Action signature to include 'bool isReplay'
    public void VerifyLocation(string locationId, Action<bool, string, bool> onResult)
    {
        StartCoroutine(VerifyLocationRoutine(locationId, onResult));
    }

    IEnumerator VerifyLocationRoutine(string locationId, Action<bool, string, bool> onResult)
    {
        string url = $"{baseUrl}/verify-location";
        string jsonBody = $"{{\"scannedLocationId\": \"{locationId}\"}}";

        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            
            request.SetRequestHeader("Authorization", "Bearer " + GlobalGameState.playerToken);
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                string json = request.downloadHandler.text;
                Debug.Log($"[API RAW] {json}"); 
                
                try 
                {
                    // Use Proper JSON Parsing
                    VerifyResponse response = JsonUtility.FromJson<VerifyResponse>(json);
                    
                    // Backend sends: success, message, alreadyCompleted
                    bool isReplay = response.alreadyCompleted;

                    if (response.success)
                    {
                        onResult?.Invoke(true, response.message, isReplay);
                    }
                    else
                    {
                        onResult?.Invoke(false, response.message, false);
                    }
                }
                catch
                {
                    Debug.LogError("JSON Parse Fail on Verify");
                    onResult?.Invoke(false, "Data Error", false);
                }
            }
            else
            {
                Debug.LogError($"[API ERROR] {request.responseCode}");
                onResult?.Invoke(false, "Network Error", false);
            }
        }
    }
    
    // --- Helper Classes ---
    [Serializable]
    public class VerifyResponse
    {
        public bool success;
        public string message;
        public bool alreadyCompleted; // Matches backend
    }

    // --- 5. LOGOUT ---
    // --- 5. LOGOUT ---
    public void Logout(Action<bool> onResult)
    {
        StartCoroutine(LogoutRoutine(onResult));
    }

    IEnumerator LogoutRoutine(Action<bool> onResult)
    {
        // 1. CONSTRUCT CORRECT URL
        // We grab just the "http://localhost:3000/api" part and add "/logout"
        // This fixes the issue regardless of whether baseUrl is ".../game" or ".../auth"
        string rootUrl = new Uri(baseUrl).GetLeftPart(UriPartial.Authority); 
        string logoutUrl = rootUrl + "/logout"; // Result: http://localhost:3000/api/logout

        Debug.Log($"[API] Logging out via: {logoutUrl}");

        // 2. PREPARE JSON BODY (CRITICAL)
        // We MUST tell the backend WHICH device to remove.
        string jsonBody = $"{{\"deviceId\": \"{GlobalGameState.deviceId}\"}}";
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);

        using (UnityWebRequest request = new UnityWebRequest(logoutUrl, "POST"))
        {
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            
            // 3. ATTACH HEADERS (Required for 'requireAuth')
            request.SetRequestHeader("Authorization", "Bearer " + GlobalGameState.playerToken);
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("[API] Logout Successful. Device removed from DB.");
                onResult?.Invoke(true);
            }
            else
            {
                Debug.LogError($"[API ERROR] Logout Failed: {request.error}");
                Debug.LogError($"[API RESPONSE] {request.downloadHandler.text}");
                
                // Force success locally so the user isn't stuck
                onResult?.Invoke(false); 
            }
        }
    }

}
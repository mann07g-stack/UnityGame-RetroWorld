using UnityEngine;
using System.Collections;

public class AutoLogoutOnQuit : MonoBehaviour
{
    private bool isSafeToQuit = false;

    private void Start()
    {
        // Subscribe to the Quit event so we can intercept it
        Application.wantsToQuit += OnWantToQuit;
    }

    private bool OnWantToQuit()
    {
        // 1. If we already finished logging out, allow the app to close
        if (isSafeToQuit) return true;

        // 2. If we aren't even logged in, just close immediately
        if (string.IsNullOrEmpty(GlobalGameState.playerToken)) return true;

        // 3. Otherwise, CANCEL the close request and start the logout sequence
        StartCoroutine(LogoutAndExitSequence());
        
        // Return 'false' tells Unity: "Wait! Don't close yet!"
        return false; 
    }

    private IEnumerator LogoutAndExitSequence()
    {
        Debug.Log("⚠️ Application Closing... Sending Logout Request.");

        bool apiFinished = false;

        // Call the API Manager Logout (which sends the Device ID)
        if (APIManager.Instance != null)
        {
            APIManager.Instance.Logout((success) => 
            {
                apiFinished = true;
            });
        }
        else
        {
            apiFinished = true;
        }

        // Wait for the API to finish OR a 2-second timeout (safety net)
        // We use unscaled time so it runs even if Time.timeScale is 0
        float timer = 0f;
        while (!apiFinished && timer < 2.0f)
        {
            timer += Time.unscaledDeltaTime; 
            yield return null;
        }

        // Now that the backend knows we left, allow the quit
        isSafeToQuit = true;
        Debug.Log("✅ Logout Complete. Closing Game.");
        
        // Trigger the quit again (this time it will pass the check above)
        Application.Quit();
    }
}
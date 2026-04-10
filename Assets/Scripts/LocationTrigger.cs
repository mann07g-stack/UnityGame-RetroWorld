using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement; 

public class LocationTrigger : MonoBehaviour
{
    [Header("Configuration")]
    public string locationID; 
    public string sceneToLoad = "Test_NPC"; 
    public string winSceneToLoad = "Test_NPC_Win"; 
    public GameInteractionData gameDataForThisSpot;

    private bool isChecking = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !isChecking)
        {   
            Debug.Log($"🚨 CHECKING LOCATION: Sending ID '{locationID}' to Backend...");
            if (GlobalGameState.lastTriggeredLocation == locationID) return;

            // --- NEW: CALCULATE SAFE RETURN POSITION ---
            // 1. Get direction from Trigger Center -> Player
            Vector3 directionAway = (other.transform.position - transform.position).normalized;
            
            // 2. Push player 1.5 units away along that direction
            // We strip the Y (height) so they don't get pushed into the sky or floor
            directionAway.y = 0; 
            
            GlobalGameState.playerReturnPosition = other.transform.position + (directionAway * 1.5f);
            
            // Debug to see where they will land
            Debug.Log($"📍 Saved Return Position: {GlobalGameState.playerReturnPosition}");
            // -------------------------------------------

            StartCoroutine(CheckAndLoad());
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") && GlobalGameState.lastTriggeredLocation == locationID)
        {
            GlobalGameState.lastTriggeredLocation = "";
            Debug.Log("🔄 Reset location lock.");
        }
    }

    IEnumerator CheckAndLoad()
    {
        isChecking = true;
        GlobalGameState.currentScannedLocation = locationID;
        
        if (APIManager.Instance == null) { isChecking = false; yield break; }

        bool apiFinished = false;
        bool isValid = false;
        bool isReplay = false;

        APIManager.Instance.VerifyLocation(locationID, (success, msg, replay) => 
        {
            isValid = success;
            isReplay = replay;
            apiFinished = true;
        });

        float timer = 0f;
        while (!apiFinished && timer < 5f) { timer += Time.deltaTime; yield return null; }

        if (!apiFinished) { isChecking = false; yield break; }

        if (isValid)
        {
            GlobalGameState.lastTriggeredLocation = locationID;
            GlobalGameState.activeGameData = gameDataForThisSpot;

            if (isReplay)
            {
                if (Application.CanStreamedLevelBeLoaded(winSceneToLoad))
                    SceneManager.LoadScene(winSceneToLoad);
            }
            else
            {
                SceneManager.LoadScene(sceneToLoad);
            }
        }
        else
        {
            Debug.LogWarning("Location Check Failed");
        }

        yield return new WaitForSeconds(2f);
        isChecking = false;
    }
}
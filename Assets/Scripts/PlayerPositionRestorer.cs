using UnityEngine;
using UnityEngine.SceneManagement; // <--- REQUIRED
using System.Collections;

public class PlayerPositionRestorer : MonoBehaviour
{
    private void OnEnable()
    {
        // Subscribe to scene loading events
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // This method signature MUST match exactly for the event to work
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (GlobalGameState.isReturningFromGame)
        {
            StartCoroutine(RestoreRoutine());
        }
    }

    IEnumerator RestoreRoutine()
    {
        // 1. Wait for UI initialization
        yield return new WaitForSeconds(0.1f);

        // 2. Find InteractionManager
        InteractionManager im = FindFirstObjectByType<InteractionManager>();
        
        if (im != null)
        {
            Debug.Log("✅ Player returned. Triggering NPC Win Dialogue...");
            im.PlayPostGameDialogue(); 
        }
        else
        {
            // If we are in MainMap, it's okay if not found immediately, 
            // but if in NPC scene, this warns us.
            Debug.Log("InteractionManager not found in this scene.");
        }

        // 3. Reset Flag
        GlobalGameState.isReturningFromGame = false;
    }
}
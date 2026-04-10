using UnityEngine;

public class MiniGameTrigger : MonoBehaviour
{
    public GameInteractionData gameData; 
    public InteractionManager uiManager; 

    private bool hasPlayed = false;

    private void OnTriggerEnter(Collider other)
    {
        // DEBUG 1: Did physics work?
        Debug.Log("Something hit the trigger! It was: " + other.name);

        if (other.CompareTag("Player"))
        {
            // DEBUG 2: Did the tag check pass?
            Debug.Log("Tag verified. Checking checks...");

            if (!hasPlayed)
            {
                if(uiManager != null)
                {
                    Debug.Log("Everything good. Starting Interaction.");
                    uiManager.StartInteraction(gameData);
                    hasPlayed = true; 
                }
                else
                {
                    Debug.LogError("CRITICAL ERROR: The 'UI Manager' slot is empty on this Cube!");
                }
            }
            else
            {
                Debug.Log("Ignored because this game was already played.");
            }
        }
        else
        {
             Debug.LogError("Tag Mismatch! expected 'Player', but hit object has tag: " + other.tag);
        }
    }
}
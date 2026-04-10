using UnityEngine;
using UnityEngine.SceneManagement;

public class DynamicNPCSetup : MonoBehaviour
{
    void Start()
    {
        if (GlobalGameState.activeGameData != null)
        {
            // 1. Setup Data
            MiniGameTrigger trigger = GetComponent<MiniGameTrigger>();
            if (trigger != null)
            {
                trigger.gameData = GlobalGameState.activeGameData;
            }

            // 2. CHECK: Did we just come back from winning the game?
            if (GlobalGameState.isReturningFromGame)
            {
                Debug.Log("🏆 Returning from Game: Auto-playing Win Dialogue...");
                
                // Reset the flag so it doesn't happen again
                GlobalGameState.isReturningFromGame = false;

                if (InteractionManager.Instance != null)
                {
                    // This function reads 'activeGameData.winSentences'
                    InteractionManager.Instance.PlayPostGameDialogue();
                }
            }
        }
    }
}
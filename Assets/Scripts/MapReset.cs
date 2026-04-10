using UnityEngine;

public class MapReset : MonoBehaviour
{
    void Awake()
    {
        // 1. Unfreeze Time (Fixes physics issues)
        Time.timeScale = 1.0f; 
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // 2. CHECK FOR SAVED POSITION
        if (GlobalGameState.playerReturnPosition != Vector3.zero)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            
            if (player != null)
            {
                Debug.Log($"🌍 Teleporting Player to Safe Spot: {GlobalGameState.playerReturnPosition}");

                // --- CRITICAL FOR CHARACTER CONTROLLERS ---
                // If you use a CharacterController, you MUST disable it before moving
                // or it will override your teleport and snap back.
                CharacterController cc = player.GetComponent<CharacterController>();
                if (cc != null) cc.enabled = false;

                // Teleport
                player.transform.position = GlobalGameState.playerReturnPosition;

                // Re-enable
                if (cc != null) cc.enabled = true;

                // 3. Clear the position so it doesn't happen on normal game start
                GlobalGameState.playerReturnPosition = Vector3.zero;
            }
        }
    }
}
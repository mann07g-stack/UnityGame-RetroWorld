using UnityEngine;
using UnityEngine.SceneManagement;

public class collison : MonoBehaviour

{
     private bool hasCollidedThisLevel = false;
    CubaceAudioManager audioManager;
    private void Awake()
    {
        // Find the AudioManager by tag
        audioManager = GameObject.FindGameObjectWithTag("Audio").GetComponent<CubaceAudioManager>();
    }



    private void OnEnable()
    {
        hasCollidedThisLevel = false; // ✅ Reset flag on level load
    }





    void OnCollisionEnter(Collision CollisionInfo)
    {
        if (hasCollidedThisLevel) return;
        
        if (CollisionInfo.collider.CompareTag("obstacle"))
        {
            hasCollidedThisLevel = true;
            audioManager.StopMusic(); // Stop background music
            audioManager.PlaySFX(audioManager.death);

            // Increment collision count
            GameStatsManager.Instance.IncrementCollision();
            GameStatsManager.Instance.isRestartingFromCollision = true;
            

            // Disable player movement so camera doesn't follow to spawn
            var player = FindFirstObjectByType<PlayerMovementSingle>();
            if (player != null)
                player.enabled = false;

            // Optional: stop player's Rigidbody too
            var rb = player.GetComponent<Rigidbody>();
            if (rb != null)
                rb.linearVelocity = Vector3.zero;

            // Shake the screen
            if (ScreenShake.Instance != null)
                ScreenShake.Instance.Shake(0.2f, 0.2f);

            // Restart game after delay
            var gameManager = FindFirstObjectByType<CubaceGameManager>();
            if (gameManager != null)
                gameManager.EndGame();
        }
    }
}

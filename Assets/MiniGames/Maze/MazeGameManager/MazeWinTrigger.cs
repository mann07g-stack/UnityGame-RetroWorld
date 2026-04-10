using UnityEngine;

public class MazeWinTrigger : MonoBehaviour
{
    private bool hasTriggered = false;

    void OnTriggerEnter(Collider other)
    {
        if (hasTriggered) return;

        // Check if the object hitting the wall is the Player
        if (other.CompareTag("Player"))
        {
            hasTriggered = true;
            Debug.Log("🏁 Wall Reached!");

            if (MazeGameManager.Instance != null)
            {
                MazeGameManager.Instance.WinGame();
            }
            else
            {
                Debug.LogError("MazeGameManager is missing from the scene!");
            }
        }
    }
}
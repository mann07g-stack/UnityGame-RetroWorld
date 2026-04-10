using UnityEngine;

public class EndTrigger : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        // 1. STRICT CHECK: Only the Player can trigger the win
        if (other.CompareTag("Player"))
        {
            Debug.Log("🏁 Player reached the Finish Line!");
            
            if (CubaceGameManager.Instance != null)
            {
                CubaceGameManager.Instance.CompleteLevel(); // Calls WIN
            }
        }
    }
}
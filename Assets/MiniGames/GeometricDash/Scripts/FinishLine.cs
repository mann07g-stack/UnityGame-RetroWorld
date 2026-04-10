using UnityEngine;

public class FinishLine : MonoBehaviour
{
    // We removed the local UI references because the Manager handles them now
    
    void OnTriggerEnter2D(Collider2D other)
    {
        // 1. Check if it is the player
        if (other.GetComponent<GeomPlayerController>() != null)
        {
            Debug.Log("🏁 Player hit Finish Line!");

            // 2. Tell the Manager we won
            if (GeomGameManager.Instance != null)
            {
                GeomGameManager.Instance.LevelComplete();
            }
            else
            {
                Debug.LogError("GeomGameManager is missing from the scene!");
            }
        }
    }
}
using UnityEngine;

public class FinishWall : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player")) 
        {
            Debug.Log("Goal Reached!");

            CrossyPlayerController pc = other.GetComponent<CrossyPlayerController>();
            if (pc != null) pc.isAlive = false;

            if (CrossyGameManager.Instance != null)
            {
                CrossyGameManager.Instance.WinGame();
            }
        }
    }
}
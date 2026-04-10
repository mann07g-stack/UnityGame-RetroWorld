using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class LevelLoader : MonoBehaviour
{
    [Header("Settings")]
    public string normalScene = "Test_NPC";
    public string winScene = "Test_NPC_Win";
    public string gameNameID = "FlappyBird"; 

    private bool canTravel = false; // Starts Locked

    private void Start()
    {
        // Wait 2 seconds before this portal becomes active
        // This gives you time to walk away if you spawned nearby
        StartCoroutine(EnableTravelRoutine());
    }

    IEnumerator EnableTravelRoutine()
    {
        yield return new WaitForSeconds(2.0f);
        canTravel = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        // 1. If portal is not ready yet, ignore everything
        if (!canTravel) return;

        if (other.CompareTag("Player"))
        {
            if (GlobalGameState.completedGames.Contains(gameNameID))
            {
                Debug.Log("Game already done. Redirecting to Win Scene...");
                SceneManager.LoadScene(winScene);
            }
            else
            {
                Debug.Log("Teleporting to Level...");
                SceneManager.LoadScene(normalScene);
            }
        }
    }
}
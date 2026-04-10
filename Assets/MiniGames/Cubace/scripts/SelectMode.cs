using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;


public class SelectMode : MonoBehaviour
{
void Start()
{
    StartCoroutine(ValidatePlayer());
}

private IEnumerator ValidatePlayer()
{
    yield return new WaitForSeconds(0.1f); // Give Unity time to initialize DontDestroyOnLoad objects

    if (PlayerDataManager.Instance == null || !PlayerDataManager.Instance.IsCurrentPlayerValid())
    {
        Debug.LogWarning("No player name set or PlayerDataManager missing. Redirecting to main menu.");
        SceneManager.LoadScene(0);
    }
}

    

    // Call this from a button to start single player mode
    public void Single_Mode()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }
}

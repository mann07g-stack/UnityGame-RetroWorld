using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections; // <-- Add this line

public class LoadingScreen : MonoBehaviour
{
    public float waitTime = 3f; // seconds to wait

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartCoroutine(LoadNextSceneAfterDelay());
    }

    IEnumerator LoadNextSceneAfterDelay()
    {
        yield return new WaitForSeconds(waitTime);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    
   
}

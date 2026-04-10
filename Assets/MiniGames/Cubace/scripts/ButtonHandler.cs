using UnityEngine;
using UnityEngine.SceneManagement;

public class ButtonHandler : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void NewPlayerClicked()
    {
        SceneManager.LoadScene("NewPlayer");
    }
    public void OnRetryClicked()
    {
        SceneManager.LoadScene("Retry");
    }
    public void OnMainMenuClicked()
    {
        SceneManager.LoadScene("menu");
    }
    public void OnQuitClicked()
    {
        Debug.Log("Quitting the game...");
        Application.Quit();
    }
    public void OnBackClicked()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex-1);
    }
}

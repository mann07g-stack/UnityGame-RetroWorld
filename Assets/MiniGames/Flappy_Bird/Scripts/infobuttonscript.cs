using UnityEngine;

public class InfoButtonVisibility : MonoBehaviour
{
    void Update()
    {
        // Show info button ONLY when the game is paused
        // (Geometry Dash behavior)
        if (Time.timeScale == 0f)
        {
            if (!gameObject.activeSelf)
                gameObject.SetActive(true);
        }
        else
        {
            if (gameObject.activeSelf)
                gameObject.SetActive(false);
        }
    }
}

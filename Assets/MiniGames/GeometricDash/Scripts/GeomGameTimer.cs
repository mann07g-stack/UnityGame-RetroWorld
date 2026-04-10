using UnityEngine;
using UnityEngine.UI; 

public class GeomGameTimer : MonoBehaviour
{
    [Header("UI Reference")]
    public Text timerText; 

    // Static variables persist across scene reloads
    public static float elapsedTime = 0f; 
    public static bool isLevelComplete = false;

    void Awake()
    {
        // We do NOT reset here, because we might want the timer to persist 
        // across deaths if you want "Total Time". 
        // However, GeomGameManager now calls ResetTimer() on Start() 
        // so every attempt is fresh.
    }

    public void PauseTimer()
    {
        enabled = false;
    }

    public void ResumeTimer()
    {
        enabled = true;
    }
    
    void Update()
    {
        // Stop counting if level is done
        if (isLevelComplete) return;

        elapsedTime += Time.deltaTime;
        UpdateTimerUI();

        // DEV TOOL: Reset
        if (Input.GetKeyDown(KeyCode.R))
        {
            ResetTimer();
        }
    }

    void UpdateTimerUI()
    {
        int minutes = Mathf.FloorToInt(elapsedTime / 60F);
        int seconds = Mathf.FloorToInt(elapsedTime % 60F);
        int milliseconds = Mathf.FloorToInt((elapsedTime * 100F) % 100F);

        if (timerText != null)
        {
            timerText.text = string.Format("{0:00}:{1:00}:{2:00}", minutes, seconds, milliseconds);
        }
    }

    public static void ResetTimer()
    {
        elapsedTime = 0f;
        isLevelComplete = false;
    }
}
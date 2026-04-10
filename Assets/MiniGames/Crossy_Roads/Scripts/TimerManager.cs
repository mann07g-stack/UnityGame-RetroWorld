using UnityEngine;
using UnityEngine.UI;

public class TimerManager : MonoBehaviour
{
    public static TimerManager Instance;
    public Text timerText;
    
    public float timeElapsed;
    public float targetGoalTime = 120f;

    // This is the key to stopping the timer
    private bool timerRunning = true; 

    void Awake()
    {
        Instance = this;
    }

    void Update()
    {
        // Only count time if the timer is running
        if (timerRunning) 
        {
            timeElapsed += Time.deltaTime;
            UpdateUI();
        }
    }

    void UpdateUI()
{
    if (timerText == null) return;

    int minutes = Mathf.FloorToInt(timeElapsed / 60f);
    int seconds = Mathf.FloorToInt(timeElapsed % 60f);
    int milliseconds = Mathf.FloorToInt((timeElapsed * 1000f) % 1000f / 10f);
    // /10f → converts to 2-digit milliseconds (00–99)

    timerText.text = string.Format("{0:00}:{1:00}:{2:00}",
                                   minutes,
                                   seconds,
                                   milliseconds);
}

    // Call this from your FinishWall script
    public void StopTimer()
    {
        timerRunning = false;
    }
}
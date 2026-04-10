using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class GameTimer : MonoBehaviour
{
    public static GameTimer Instance;

    public TextMeshProUGUI timerText;
    public float timeElapsed = 0f;

    private bool timerRunning = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(this.gameObject); // Keep this alive between scenes
            SceneManager.sceneLoaded += OnSceneLoaded; // Listen for scene loads
        }
        else
        {
            Destroy(gameObject); // Avoid duplicates
        }
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Find and reassign the timerText after scene loads
        if (timerText == null)
        {
            GameObject foundText = GameObject.Find("TimerText");
            if (foundText != null)
            {
                timerText = foundText.GetComponent<TextMeshProUGUI>();
            }
        }
    }

    void Update()
    {
        if (timerRunning)
        {
            timeElapsed += Time.deltaTime;
            UpdateTimerUI();
        }
    }

    void UpdateTimerUI()
    {
        if (timerText == null) return;

        int minutes = Mathf.FloorToInt(timeElapsed / 60);
        int seconds = Mathf.FloorToInt(timeElapsed % 60);
        int milliseconds = Mathf.FloorToInt((timeElapsed * 1000) % 1000);
        timerText.text = $"{minutes:00}:{seconds:00}:{milliseconds:000}";
    }
    public void StartTimer()
    {
        timerRunning = true;
    }


    public void StopTimer()
    {
        timerRunning = false;
    }

    public float GetElapsedTime()
    {
        return timeElapsed;
    }
  


}

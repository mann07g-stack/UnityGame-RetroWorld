using UnityEngine;
using TMPro;

public class UITimer : MonoBehaviour
{
    public TextMeshProUGUI timerText;

    public bool countdown = false;
    public float startTime = 0f;

    float time;
    bool running = true;

    static UITimer instance;

    void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        time = startTime;
        running = true;
        UpdateText();
    }

    void Update()
    {
        if (!running) return;

        time += countdown ? -Time.unscaledDeltaTime : Time.unscaledDeltaTime;
        UpdateText();
    }

    void UpdateText()
    {
        int min = Mathf.FloorToInt(time / 60);
        int sec = Mathf.FloorToInt(time % 60);
        timerText.text = $"{min:00}:{sec:00}";
    }

    public void StopTimer() => running = false;
    public void StartTimer() => running = true;
    public float GetTime() => time;
}

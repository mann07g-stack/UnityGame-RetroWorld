using UnityEngine;
using TMPro;

public class MineTimeManager : MonoBehaviour
{
    public TextMeshProUGUI timerText;

    private float elapsedTime = 0f;
    private bool isRunning = true;

    void Update()
    {
        if (!isRunning) return;

        elapsedTime += Time.deltaTime;
        timerText.text = Mathf.FloorToInt(elapsedTime).ToString("000");
    }

    // Used when info panel opens
    public void PauseTimer()
    {
        isRunning = false;
    }

    // Used when info panel closes OR new round starts
    public void ResumeTimer()
    {
        isRunning = true;
    }

    // Used on WIN
    public void StopTimer()
    {
        isRunning = false;
    }
}

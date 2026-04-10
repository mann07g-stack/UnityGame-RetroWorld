using UnityEngine;

public class MineGameManager : MonoBehaviour
{
    public MineTimeManager timerManager;

    // Optional helpers (not strictly required, but clean)
    public void StopTimer()
    {
        timerManager.StopTimer();
    }

    public void ResumeTimer()
    {
        timerManager.ResumeTimer();
    }
}

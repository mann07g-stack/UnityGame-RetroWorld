using UnityEngine;

public class Lightsout_Music : MonoBehaviour
{
    // Since GridManager now handles the specific game audio,
    // this script is only needed if you want persistent audio 
    // that survives scene loads (like a global background track).
    
    private static Lightsout_Music instance;

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
    }
}
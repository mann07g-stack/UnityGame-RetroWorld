using UnityEngine;

public class MastermindBGM : MonoBehaviour
{
    private static MastermindBGM instance;

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

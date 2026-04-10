using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuMusicManager : MonoBehaviour
{
    public AudioSource audioSource;
    public AudioClip menuMusic;

    public static MenuMusicManager Instance;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Keep music across scenes
        }
        else
        {
            Destroy(gameObject); // Avoid duplicate music
            return;
        }
    }

    void Start()
    {
        if (audioSource != null && menuMusic != null)
        {
            audioSource.clip = menuMusic;
            audioSource.loop = true;
            audioSource.Play();
        }

        // Listen for scene changes
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "LEVEL 01") // 👈 replace with your exact Level 1 scene name
        {
            audioSource.Stop();      // Stop the menu music
            Destroy(gameObject);     // Clean up so level music plays
        }
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}

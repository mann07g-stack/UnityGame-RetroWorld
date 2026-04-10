using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Audio Sources")]
    public AudioSource musicSource; // Dedicated source for music
    public AudioSource sfxSource;   // Dedicated source for sound effects

    [Header("Audio Clips")]
    public AudioClip backgroundMusic;
    public AudioClip hitSound;      // Sound when player dies

    void Awake()
    {
        // Singleton pattern to access this from any script
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        PlayMusic();
    }

    public void PlayMusic()
    {
        if (backgroundMusic != null && musicSource != null)
        {
            musicSource.clip = backgroundMusic;
            musicSource.loop = true; // IMPORTANT: Loop the background music
            musicSource.Play();
        }
    }

    public void PlayHitSound()
    {
        if (hitSound != null && sfxSource != null)
        {
            sfxSource.PlayOneShot(hitSound);
        }
    }
}
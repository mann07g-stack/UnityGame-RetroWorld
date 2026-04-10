using UnityEngine;

public class EndMusic : MonoBehaviour
{
    public AudioSource audioSource;
    public AudioClip menuMusic;

    void Start()
    {
        if (audioSource != null && menuMusic != null)
        {
            audioSource.clip = menuMusic;
            audioSource.loop = true;
            audioSource.Play();
        }
    }
}

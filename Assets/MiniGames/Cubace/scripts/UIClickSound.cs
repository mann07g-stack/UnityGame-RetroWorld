using UnityEngine;

public class UIClickSound : MonoBehaviour
{
    public AudioSource audioSource;

    public void PlayClickSound()
    {
        if (audioSource != null && audioSource.clip != null)
        {
            audioSource.Play();
        }
    }
}

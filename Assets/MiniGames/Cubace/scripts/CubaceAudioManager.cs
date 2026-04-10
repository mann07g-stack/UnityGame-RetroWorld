using UnityEngine;

public class CubaceAudioManager : MonoBehaviour
{
    [Header("---------Audio Sources--------")]
    [SerializeField] AudioSource musicSource;
    [SerializeField] AudioSource SFXSource;
    [Header("---------Audio Clips--------")]
    public AudioClip background;
    public AudioClip death;
    public AudioClip levelfinished;
    private void Start()
    {
        musicSource.clip = background;
        musicSource.Play();
    }
    public void PlaySFX(AudioClip clip)
    {

        SFXSource.PlayOneShot(clip);

    }  
    public void StopMusic()
    {
        musicSource.Stop();
    }



}
